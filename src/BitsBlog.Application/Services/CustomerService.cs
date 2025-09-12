using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BitsBlog.Application.Interfaces;
using BitsBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BitsBlog.Application.Common;
using BitsBlog.Application.DTO;

namespace BitsBlog.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> _repo;
        public CustomerService(IRepository<Customer> repo)
        {
            _repo = repo;
        }

        public async Task<BitsBlog.Application.DTO.ResultDto> EnsureAdminAsync(BitsBlog.Application.DTO.AdminSeedDto dto, System.Threading.CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BitsBlog.Application.DTO.ResultDto.Fail("Email/password required");
            var loginId = NormalizeEmail(dto.Email);
            var exists = await _repo.AsNoTracking().AnyAsync(c => c.LoginId == loginId, ct);
            if (exists) return BitsBlog.Application.DTO.ResultDto.Success();

            var (hash, salt) = HashPassword(dto.Password);
            var admin = new Customer
            {
                LoginId = loginId,
                DisplayName = string.IsNullOrWhiteSpace(dto.DisplayName) ? loginId : dto.DisplayName.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = "Admin",
                Created = DateTime.UtcNow
            };
            await _repo.ExecuteInTransactionAsync(async _ =>
            {
                await _repo.InsertAsync(admin);
            });
            return BitsBlog.Application.DTO.ResultDto.Success();
        }

        public async Task<BitsBlog.Application.DTO.ResultDto<BitsBlog.Application.DTO.AuthUserDto>> RegisterAsync(BitsBlog.Application.DTO.RegisterDto dto, System.Threading.CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BitsBlog.Application.DTO.ResultDto<BitsBlog.Application.DTO.AuthUserDto>.Fail("Email/password required");
            var loginId = NormalizeEmail(dto.Email);
            var exists = await _repo.AsNoTracking().AnyAsync(c => c.LoginId == loginId, ct);
            if (exists) return BitsBlog.Application.DTO.ResultDto<BitsBlog.Application.DTO.AuthUserDto>.Fail("Email already registered");

            var (hash, salt) = HashPassword(dto.Password);
            var customer = new Customer
            {
                LoginId = loginId,
                DisplayName = string.IsNullOrWhiteSpace(dto.DisplayName) ? loginId : dto.DisplayName!.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt,
                Role = "User",
                Created = DateTime.UtcNow
            };
            await _repo.ExecuteInTransactionAsync(async _ =>
            {
                await _repo.InsertAsync(customer, ct);
            }, ct);
            var userDto = new BitsBlog.Application.DTO.AuthUserDto { Id = customer.Id, LoginId = customer.LoginId, DisplayName = customer.DisplayName, Role = customer.Role };
            return BitsBlog.Application.DTO.ResultDto<BitsBlog.Application.DTO.AuthUserDto>.Success(userDto);
        }

        public async Task<BitsBlog.Application.DTO.ResultDto<BitsBlog.Application.DTO.AuthUserDto>> LoginAsync(BitsBlog.Application.DTO.LoginDto dto, System.Threading.CancellationToken ct = default)
        {
            var loginId = NormalizeEmail(dto.Email ?? string.Empty);
            var customer = await _repo.AsNoTracking().FirstOrDefaultAsync(c => c.LoginId == loginId, ct);
            if (customer is null) return BitsBlog.Application.DTO.ResultDto<BitsBlog.Application.DTO.AuthUserDto>.Fail("Invalid credentials");
            if (!VerifyPassword(dto.Password ?? string.Empty, customer.PasswordHash, customer.PasswordSalt))
                return BitsBlog.Application.DTO.ResultDto<BitsBlog.Application.DTO.AuthUserDto>.Fail("Invalid credentials");
            var userDto2 = new BitsBlog.Application.DTO.AuthUserDto { Id = customer.Id, LoginId = customer.LoginId, DisplayName = customer.DisplayName, Role = customer.Role };
            return BitsBlog.Application.DTO.ResultDto<BitsBlog.Application.DTO.AuthUserDto>.Success(userDto2);
        }

        public async Task<BitsBlog.Application.DTO.ProfileDto?> GetProfileAsync(BitsBlog.Application.DTO.ProfileQueryDto query, System.Threading.CancellationToken ct = default)
        {
            var id = NormalizeEmail(query.LoginId ?? string.Empty);
            var q = _repo.AsNoTracking().Where(c => c.LoginId == id)
                .Select(c => new BitsBlog.Application.DTO.ProfileDto { LoginId = c.LoginId, DisplayName = c.DisplayName, Role = c.Role, Created = c.Created });
            var r = await q.FirstOrDefaultAsync(ct);
            return r;
        }

        public async Task<BitsBlog.Application.DTO.Common.PagedResult<BitsBlog.Application.DTO.UserDto>> ListUsersAsync(BitsBlog.Application.DTO.UserQueryDto queryParams, System.Threading.CancellationToken ct = default)
        {
            var pageSize = queryParams.PageSize;
            if (pageSize <= 0) pageSize = 100;
            if (pageSize > 500) pageSize = 500;

            var q = _repo.AsNoTracking();


            // Sort and page at entity level
            var sort = (queryParams.Sort ?? string.Empty).Trim().ToLowerInvariant();
            bool desc = sort.EndsWith("_desc");
            if (sort.StartsWith("created"))
                q = desc ? q.OrderByDescending(c => c.Created) : q.OrderBy(c => c.Created);
            else if (sort.StartsWith("login"))
                q = desc ? q.OrderByDescending(c => c.LoginId) : q.OrderBy(c => c.LoginId);
            else if (sort.StartsWith("name"))
                q = desc ? q.OrderByDescending(c => c.DisplayName) : q.OrderBy(c => c.DisplayName);
            else
                q = q.OrderBy(c => c.Id);

            var page = queryParams.Page <= 0 ? 1 : queryParams.Page; var total = await q.CountAsync(ct);

            return await _repo.PagedAsync<Customer, BitsBlog.Application.DTO.UserDto>(q.Select(c => new BitsBlog.Application.DTO.UserDto(c.Id, c.LoginId, c.DisplayName, c.Role, c.Created)), page, pageSize);
        }

        public async Task<BitsBlog.Application.DTO.UserDto?> GetUserByIdAsync(int id, System.Threading.CancellationToken ct = default)
        {
            var q = _repo.AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new BitsBlog.Application.DTO.UserDto(c.Id, c.LoginId, c.DisplayName, c.Role, c.Created));
            return await q.FirstOrDefaultAsync(ct);
        }

        public async Task<BitsBlog.Application.DTO.UserDto?> GetUserByIdAsync(int id)
        {
            var q = _repo.AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new BitsBlog.Application.DTO.UserDto(c.Id, c.LoginId, c.DisplayName, c.Role, c.Created));
            return await q.FirstOrDefaultAsync();
        }

        public Task<int> CountUsersAsync(BitsBlog.Application.DTO.UserQueryDto queryParams, System.Threading.CancellationToken ct = default)
        {
            var query = _repo.AsNoTracking();

            return query.CountAsync(ct);
        }

        public async Task<bool> SetRoleAsync(int id, string role, System.Threading.CancellationToken ct = default)
        {
            role = (role ?? string.Empty).Trim();
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(role, "User", StringComparison.OrdinalIgnoreCase))
                return false;
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;
            entity.Role = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";
            await _repo.ExecuteInTransactionAsync(async _ =>
            {
                await _repo.UpdateAsync(entity, ct);
            }, ct);
            return true;
        }

        public async Task<bool> DeleteUserAsync(int id, System.Threading.CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;
            await _repo.ExecuteInTransactionAsync(async _ =>
            {
                await _repo.DeleteAsync(entity, ct);
            }, ct);
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

        public async Task<BitsBlog.Application.DTO.ResultDto> UpdateDisplayNameAsync(BitsBlog.Application.DTO.UpdateProfileDto dto, System.Threading.CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.DisplayName)) return BitsBlog.Application.DTO.ResultDto.Fail("DisplayName required");
            if (dto.DisplayName.Length > 100) return BitsBlog.Application.DTO.ResultDto.Fail("DisplayName too long");
            var id = NormalizeEmail(dto.LoginId ?? string.Empty);
            var entity = await _repo.AsTracking().FirstOrDefaultAsync(c => c.LoginId == id, ct);
            if (entity is null) return BitsBlog.Application.DTO.ResultDto.Fail("Not found");
            entity.DisplayName = dto.DisplayName.Trim();
            await _repo.ExecuteInTransactionAsync(async _ =>
            {
                await _repo.UpdateAsync(entity, ct);
            }, ct);
            return BitsBlog.Application.DTO.ResultDto.Success();
        }

        public async Task<BitsBlog.Application.DTO.ResultDto> ChangePasswordAsync(BitsBlog.Application.DTO.ChangePasswordDto dto, System.Threading.CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                return BitsBlog.Application.DTO.ResultDto.Fail("New password too short");
            var id = NormalizeEmail(dto.LoginId ?? string.Empty);
            var entity = await _repo.AsTracking().FirstOrDefaultAsync(c => c.LoginId == id, ct);
            if (entity is null) return BitsBlog.Application.DTO.ResultDto.Fail("Not found");
            if (!VerifyPassword(dto.CurrentPassword ?? string.Empty, entity.PasswordHash, entity.PasswordSalt))
                return BitsBlog.Application.DTO.ResultDto.Fail("Invalid current password");
            var (hash, salt) = HashPassword(dto.NewPassword);
            entity.PasswordHash = hash;
            entity.PasswordSalt = salt;
            await _repo.ExecuteInTransactionAsync(async _ =>
            {
                await _repo.UpdateAsync(entity, ct);
            }, ct);
            return BitsBlog.Application.DTO.ResultDto.Success();
        }
    }
}

