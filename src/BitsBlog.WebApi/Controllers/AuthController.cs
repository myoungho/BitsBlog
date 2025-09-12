using BitsBlog.Application.Interfaces;
using BitsBlog.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BitsBlog.Application.DTO;

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

        /// <summary>Register</summary>
        [HttpPost("register")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required");

            var reg = await _customers.RegisterAsync(request, HttpContext.RequestAborted);
            if (!reg.Ok || reg.Data is null)
            {
                if (string.Equals(reg.Error, "Email already registered", StringComparison.OrdinalIgnoreCase))
                    return Conflict(reg.Error);
                return BadRequest(reg.Error ?? "Registration failed");
            }

            var token = GenerateJwt(new Customer { Id = reg.Data.Id ?? 0, LoginId = reg.Data.LoginId, DisplayName = reg.Data.DisplayName, Role = reg.Data.Role });
            return Ok(new AuthResponse(token.Token, token.Expires, reg.Data.Role, reg.Data.DisplayName));
        }

        /// <summary>Login</summary>
        [HttpPost("login")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginDto request)
        {
            var result = await _customers.LoginAsync(request, HttpContext.RequestAborted);
            if (!result.Ok || result.Data is null) return Unauthorized();
            var token = GenerateJwt(new Customer { Id = result.Data.Id ?? 0, LoginId = result.Data.LoginId, DisplayName = result.Data.DisplayName, Role = result.Data.Role });
            return Ok(new AuthResponse(token.Token, token.Expires, result.Data.Role, result.Data.DisplayName));
        }

        /// <summary>Get my profile</summary>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<object>> Me()
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var profile = await _customers.GetProfileAsync(new ProfileQueryDto { LoginId = email }, HttpContext.RequestAborted);
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

        public record AuthResponse(string AccessToken, DateTime Expires, string Role, string DisplayName);

        /// <summary>Update profile (DisplayName)</summary>
        [Authorize]
        [HttpPut("profile")]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(AuthResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AuthResponse>> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            var loginId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            request.LoginId = loginId;
            var up = await _customers.UpdateDisplayNameAsync(request, HttpContext.RequestAborted);
            if (!up.Ok) return BadRequest(up.Error ?? "Update failed");
            var me = await _customers.GetProfileAsync(new ProfileQueryDto { LoginId = loginId }, HttpContext.RequestAborted);
            if (me is null) return Unauthorized();
            int.TryParse(User.FindFirstValue("cid"), out var currentId);
            var token = GenerateJwt(new Customer
            {
                Id = currentId,
                LoginId = me.LoginId,
                DisplayName = me.DisplayName,
                Role = me.Role
            });
            return Ok(new AuthResponse(token.Token, token.Expires, me.Role, me.DisplayName));
        }

        /// <summary>Change password</summary>
        [Authorize]
        [HttpPut("password")]
        [Consumes("application/json")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var loginId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            request.LoginId = loginId;
            var cp = await _customers.ChangePasswordAsync(request, HttpContext.RequestAborted);
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
