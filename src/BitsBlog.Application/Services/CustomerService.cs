using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BitsBlog.Application.Interfaces;
using BitsBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BitsBlog.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _repo;
        public CustomerService(IRepository<Customer> repo)
        {
            _repo = repo;
        }

        public async Task<(bool ok, string? error)> EnsureAdminAsync(string email, string password, string displayName)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "Email/password required");
            var loginId = NormalizeEmail(email);
            var exists = await _repo.AsNoTracking().AnyAsync(c => c.LoginId == loginId);
            if (exists) return (true, null);

            var (hash, salt) = HashPassword(password);
            var admin = new Customer
            {
                LoginId = loginId,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? loginId : displayName.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = "Admin",
                Created = DateTime.UtcNow
            };
            await _repo.InsertAsync(admin);
            await _repo.SaveDbContextChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? error, Customer? customer)> RegisterAsync(string email, string password, string? displayName)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "Email/password required", null);
            var loginId = NormalizeEmail(email);
            var exists = await _repo.AsNoTracking().AnyAsync(c => c.LoginId == loginId);
            if (exists) return (false, "Email already registered", null);

            var (hash, salt) = HashPassword(password);
            var customer = new Customer
            {
                LoginId = loginId,
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? loginId : displayName!.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = "User",
                Created = DateTime.UtcNow
            };
            await _repo.InsertAsync(customer);
            await _repo.SaveDbContextChangesAsync();
            return (true, null, customer);
        }

        public async Task<(bool ok, string? error, Customer? customer)> LoginAsync(string email, string password)
        {
            var loginId = NormalizeEmail(email ?? string.Empty);
            var customer = await _repo.AsNoTracking().FirstOrDefaultAsync(c => c.LoginId == loginId);
            if (customer is null) return (false, "Invalid credentials", null);
            if (!VerifyPassword(password ?? string.Empty, customer.PasswordHash, customer.PasswordSalt))
                return (false, "Invalid credentials", null);
            return (true, null, customer);
        }

        public async Task<(string LoginId, string DisplayName, string Role, DateTime Created)?> GetProfileAsync(string loginId)
        {
            var id = NormalizeEmail(loginId ?? string.Empty);
            var q = _repo.AsNoTracking().Where(c => c.LoginId == id)
                .Select(c => new { c.LoginId, c.DisplayName, c.Role, c.Created });
            var r = await q.FirstOrDefaultAsync();
            return r is null ? null : (r.LoginId, r.DisplayName, r.Role, r.Created);
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        private static (string Hash, string Salt) HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);
            var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        private static bool VerifyPassword(string password, string hashBase64, string saltBase64)
        {
            try
            {
                var salt = Convert.FromBase64String(saltBase64);
                var comp = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, 100_000, HashAlgorithmName.SHA256, 32);
                return CryptographicOperations.FixedTimeEquals(comp, Convert.FromBase64String(hashBase64));
            }
            catch { return false; }
        }
    }
}

