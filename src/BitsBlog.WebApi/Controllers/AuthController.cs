using BitsBlog.Application.Services;
using BitsBlog.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BitsBlog.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ICustomerService _customers;
        private readonly IConfiguration _config;
        public AuthController(ICustomerService customers, IConfiguration config)
        {
            _customers = customers;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required");

            var reg = await _customers.RegisterAsync(request.Email, request.Password, request.DisplayName);
            if (!reg.Ok || reg.Data is null)
            {
                if (string.Equals(reg.Error, "Email already registered", StringComparison.OrdinalIgnoreCase))
                    return Conflict(reg.Error);
                return BadRequest(reg.Error ?? "Registration failed");
            }

            var token = GenerateJwt(new Customer { LoginId = reg.Data.LoginId, DisplayName = reg.Data.DisplayName, Role = reg.Data.Role });
            return Ok(new AuthResponse(token.Token, token.Expires, reg.Data.Role, reg.Data.DisplayName));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var result = await _customers.LoginAsync(request.Email, request.Password);
            if (!result.Ok || result.Data is null) return Unauthorized();
            var token = GenerateJwt(new Customer { LoginId = result.Data.LoginId, DisplayName = result.Data.DisplayName, Role = result.Data.Role });
            return Ok(new AuthResponse(token.Token, token.Expires, result.Data.Role, result.Data.DisplayName));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<object>> Me()
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var profile = await _customers.GetProfileAsync(email);
            if (profile is null) return NotFound();
            return Ok(new { profile.LoginId, profile.DisplayName, profile.Role, profile.Created });
        }

        private (string Token, DateTime Expires) GenerateJwt(Customer customer)
        {
            var issuer = _config["Jwt:Issuer"] ?? "bitsblog";
            var audience = _config["Jwt:Audience"] ?? issuer;
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
            var expiresMinutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) ? m : 60;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.LoginId),
                new Claim(ClaimTypes.Name, customer.DisplayName),
                new Claim(ClaimTypes.Role, customer.Role),
                new Claim("cid", customer.Id.ToString())
            };
            var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);
            var token = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: credentials);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return (jwt, expires);
        }

        // Password hashing/verification moved into CustomerService

        public record RegisterRequest(string Email, string Password, string? DisplayName);
        public record LoginRequest(string Email, string Password);
        public record AuthResponse(string AccessToken, DateTime Expires, string Role, string DisplayName);

        public record UpdateProfileRequest(string DisplayName);

        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<AuthResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var loginId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var up = await _customers.UpdateDisplayNameAsync(loginId, request.DisplayName ?? string.Empty);
            if (!up.Ok) return BadRequest(up.Error ?? "Update failed");
            var me = await _customers.GetProfileAsync(loginId);
            if (me is null) return Unauthorized();
            var token = GenerateJwt(new Customer
            {
                LoginId = me.LoginId,
                DisplayName = me.DisplayName,
                Role = me.Role
            });
            return Ok(new AuthResponse(token.Token, token.Expires, me.Role, me.DisplayName));
        }

        public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

        [Authorize]
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var loginId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var cp = await _customers.ChangePasswordAsync(loginId, request.CurrentPassword ?? string.Empty, request.NewPassword ?? string.Empty);
            if (!cp.Ok)
            {
                if (string.Equals(cp.Error, "Invalid current password", StringComparison.OrdinalIgnoreCase))
                    return Unauthorized();
                return BadRequest(cp.Error ?? "Change password failed");
            }
            return NoContent();
        }
    }
}
