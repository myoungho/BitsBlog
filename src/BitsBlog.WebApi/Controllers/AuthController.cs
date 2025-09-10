using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BitsBlog.Domain.Entities;
using BitsBlog.Infrastructure;
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
        private readonly BitsBlogDbContext _db;
        private readonly IConfiguration _config;
        public AuthController(BitsBlogDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required");

            var email = request.Email.Trim().ToLowerInvariant();
            var exists = await _db.Customers.AnyAsync(c => c.LoginId == email);
            if (exists) return Conflict("Email already registered");

            var (hash, salt) = HashPassword(request.Password);
            var customer = new Customer
            {
                LoginId = email,
                DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? email : request.DisplayName.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = "User",
                Created = DateTime.UtcNow
            };
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            var token = GenerateJwt(customer);
            return Ok(new AuthResponse(token.Token, token.Expires, customer.Role, customer.DisplayName));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var email = (request.Email ?? "").Trim().ToLowerInvariant();
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.LoginId == email);
            if (customer is null) return Unauthorized();
            if (!VerifyPassword(request.Password ?? string.Empty, customer.PasswordHash, customer.PasswordSalt))
                return Unauthorized();

            var token = GenerateJwt(customer);
            return Ok(new AuthResponse(token.Token, token.Expires, customer.Role, customer.DisplayName));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<object>> Me()
        {
            var email = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var user = await _db.Customers.Where(c => c.LoginId == email)
                .Select(c => new { c.LoginId, c.DisplayName, c.Role, c.Created })
                .FirstOrDefaultAsync();
            if (user is null) return NotFound();
            return Ok(user);
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

        private static (string Hash, string Salt) HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var saltBytes = new byte[16];
            rng.GetBytes(saltBytes);
            var hashBytes = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), saltBytes, 100_000, HashAlgorithmName.SHA256, 32);
            return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }

        private static bool VerifyPassword(string password, string hashBase64, string saltBase64)
        {
            try
            {
                var salt = Convert.FromBase64String(saltBase64);
                var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
                return CryptographicOperations.FixedTimeEquals(hash, Convert.FromBase64String(hashBase64));
            }
            catch { return false; }
        }

        public record RegisterRequest(string Email, string Password, string? DisplayName);
        public record LoginRequest(string Email, string Password);
        public record AuthResponse(string AccessToken, DateTime Expires, string Role, string DisplayName);
    }
}

