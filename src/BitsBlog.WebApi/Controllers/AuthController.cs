using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BitsBlog.Domain.Entities;
using BitsBlog.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

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

            var (ok, error, customer) = await _customers.RegisterAsync(request.Email, request.Password, request.DisplayName);
            if (!ok || customer is null)
            {
                if (string.Equals(error, "Email already registered", StringComparison.OrdinalIgnoreCase))
                    return Conflict(error);
                return BadRequest(error ?? "Registration failed");
            }

            var token = GenerateJwt(customer);
            return Ok(new AuthResponse(token.Token, token.Expires, customer.Role, customer.DisplayName));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var result = await _customers.LoginAsync(request.Email, request.Password);
            if (!result.ok || result.customer is null) return Unauthorized();
            var token = GenerateJwt(result.customer);
            return Ok(new AuthResponse(token.Token, token.Expires, result.customer.Role, result.customer.DisplayName));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<object>> Me()
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var profile = await _customers.GetProfileAsync(email);
            if (profile is null) return NotFound();
            return Ok(new { profile.Value.LoginId, profile.Value.DisplayName, profile.Value.Role, profile.Value.Created });
        }

        private (string Token, DateTime Expires) GenerateJwt(Customer c)
        {
            var issuer = _config["Jwt:Issuer"] ?? "bitsblog";
            var audience = _config["Jwt:Audience"] ?? issuer;
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
            var expiresMinutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) ? m : 60;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, c.LoginId),
                new Claim(ClaimTypes.Name, c.DisplayName),
                new Claim(ClaimTypes.Role, c.Role)
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
    }
}
