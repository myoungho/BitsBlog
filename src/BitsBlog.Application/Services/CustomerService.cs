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

        public async Task<BitsBlog.Application.DTOs.ResultDto> EnsureAdminAsync(string email, string password, string displayName)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return BitsBlog.Application.DTOs.ResultDto.Fail("Email/password required");
            var loginId = NormalizeEmail(email);
            var exists = await _repo.AsNoTracking().AnyAsync(c => c.LoginId == loginId);
            if (exists) return BitsBlog.Application.DTOs.ResultDto.Success();

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
            return BitsBlog.Application.DTOs.ResultDto.Success();
        }

        public async Task<BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>> RegisterAsync(string email, string password, string? displayName)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>.Fail("Email/password required");
            var loginId = NormalizeEmail(email);
            var exists = await _repo.AsNoTracking().AnyAsync(c => c.LoginId == loginId);
            if (exists) return BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>.Fail("Email already registered");

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
            var dto = new BitsBlog.Application.DTOs.AuthUserDto { Id = customer.Id, LoginId = customer.LoginId, DisplayName = customer.DisplayName, Role = customer.Role };
            return BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>.Success(dto);
        }

        public async Task<BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>> LoginAsync(string email, string password)
        {
            var loginId = NormalizeEmail(email ?? string.Empty);
            var customer = await _repo.AsNoTracking().FirstOrDefaultAsync(c => c.LoginId == loginId);
            if (customer is null) return BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>.Fail("Invalid credentials");
            if (!VerifyPassword(password ?? string.Empty, customer.PasswordHash, customer.PasswordSalt))
                return BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>.Fail("Invalid credentials");
            var dto = new BitsBlog.Application.DTOs.AuthUserDto { Id = customer.Id, LoginId = customer.LoginId, DisplayName = customer.DisplayName, Role = customer.Role };
            return BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>.Success(dto);
        }

        public async Task<BitsBlog.Application.DTOs.ProfileDto?> GetProfileAsync(string loginId)
        {
            var id = NormalizeEmail(loginId ?? string.Empty);
            var q = _repo.AsNoTracking().Where(c => c.LoginId == id)
                .Select(c => new BitsBlog.Application.DTOs.ProfileDto { LoginId = c.LoginId, DisplayName = c.DisplayName, Role = c.Role, Created = c.Created });
            var r = await q.FirstOrDefaultAsync();
            return r;
        }

        public async Task<IReadOnlyList<BitsBlog.Application.DTOs.UserDto>> ListUsersAsync(int skip = 0, int take = 100)
        {
            if (take <= 0) take = 100;
            if (take > 500) take = 500;
            var q = _repo.AsNoTracking()
                .OrderByDescending(c => c.Id)
                .Skip(skip)
                .Take(take)
                .Select(c => new BitsBlog.Application.DTOs.UserDto(c.Id, c.LoginId, c.DisplayName, c.Role, c.Created));
            return await q.ToListAsync();
        }

        public async Task<BitsBlog.Application.DTOs.UserDto?> GetUserByIdAsync(int id)
        {
            var q = _repo.AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new BitsBlog.Application.DTOs.UserDto(c.Id, c.LoginId, c.DisplayName, c.Role, c.Created));
            return await q.FirstOrDefaultAsync();
        }

        public Task<int> CountUsersAsync()
            => _repo.AsNoTracking().CountAsync();

        public async Task<bool> SetRoleAsync(int id, string role)
        {
            role = (role ?? string.Empty).Trim();
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(role, "User", StringComparison.OrdinalIgnoreCase))
                return false;
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null) return false;
            entity.Role = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";
            await _repo.UpdateAsync(entity);
            await _repo.SaveDbContextChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity is null) return false;
            await _repo.DeleteAsync(entity);
            await _repo.SaveDbContextChangesAsync();
            return true;
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

        public async Task<BitsBlog.Application.DTOs.ResultDto> UpdateDisplayNameAsync(string loginId, string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return BitsBlog.Application.DTOs.ResultDto.Fail("DisplayName required");
            if (displayName.Length > 100) return BitsBlog.Application.DTOs.ResultDto.Fail("DisplayName too long");
            var id = NormalizeEmail(loginId ?? string.Empty);
            var entity = await _repo.AsTracking().FirstOrDefaultAsync(c => c.LoginId == id);
            if (entity is null) return BitsBlog.Application.DTOs.ResultDto.Fail("Not found");
            entity.DisplayName = displayName.Trim();
            await _repo.UpdateAsync(entity);
            await _repo.SaveDbContextChangesAsync();
            return BitsBlog.Application.DTOs.ResultDto.Success();
        }

        public async Task<BitsBlog.Application.DTOs.ResultDto> ChangePasswordAsync(string loginId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                return BitsBlog.Application.DTOs.ResultDto.Fail("New password too short");
            var id = NormalizeEmail(loginId ?? string.Empty);
            var entity = await _repo.AsTracking().FirstOrDefaultAsync(c => c.LoginId == id);
            if (entity is null) return BitsBlog.Application.DTOs.ResultDto.Fail("Not found");
            if (!VerifyPassword(currentPassword ?? string.Empty, entity.PasswordHash, entity.PasswordSalt))
                return BitsBlog.Application.DTOs.ResultDto.Fail("Invalid current password");
            var (hash, salt) = HashPassword(newPassword);
            entity.PasswordHash = hash;
            entity.PasswordSalt = salt;
            await _repo.UpdateAsync(entity);
            await _repo.SaveDbContextChangesAsync();
            return BitsBlog.Application.DTOs.ResultDto.Success();
        }
    }
}
