using System;
using System.Threading.Tasks;
using BitsBlog.Domain.Entities;
using BitsBlog.Application.DTO;
using System.Collections.Generic;

namespace BitsBlog.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<ResultDto> EnsureAdminAsync(AdminSeedDto dto, System.Threading.CancellationToken ct = default);

        Task<ResultDto<AuthUserDto>> RegisterAsync(RegisterDto dto, System.Threading.CancellationToken ct = default);

        Task<ResultDto<AuthUserDto>> LoginAsync(LoginDto dto, System.Threading.CancellationToken ct = default);

        Task<ProfileDto?> GetProfileAsync(ProfileQueryDto query, System.Threading.CancellationToken ct = default);

        // Admin management
        Task<IReadOnlyList<UserDto>> ListUsersAsync(UserQueryDto query, System.Threading.CancellationToken ct = default);
        Task<UserDto?> GetUserByIdAsync(int id, System.Threading.CancellationToken ct = default);
        Task<int> CountUsersAsync(UserQueryDto query, System.Threading.CancellationToken ct = default);
        Task<bool> SetRoleAsync(int id, string role, System.Threading.CancellationToken ct = default);
        Task<bool> DeleteUserAsync(int id, System.Threading.CancellationToken ct = default);

        // Profile management
        Task<ResultDto> UpdateDisplayNameAsync(UpdateProfileDto dto, System.Threading.CancellationToken ct = default);
        Task<ResultDto> ChangePasswordAsync(ChangePasswordDto dto, System.Threading.CancellationToken ct = default);
    }
}