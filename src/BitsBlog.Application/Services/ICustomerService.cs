using System;
using System.Threading.Tasks;
using BitsBlog.Domain.Entities;

namespace BitsBlog.Application.Services
{
    public interface ICustomerService
    {
        Task<BitsBlog.Application.DTOs.ResultDto> EnsureAdminAsync(BitsBlog.Application.DTOs.AdminSeedDto dto, System.Threading.CancellationToken ct = default);

        Task<BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>> RegisterAsync(BitsBlog.Application.DTOs.RegisterDto dto, System.Threading.CancellationToken ct = default);

        Task<BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>> LoginAsync(BitsBlog.Application.DTOs.LoginDto dto, System.Threading.CancellationToken ct = default);

        Task<BitsBlog.Application.DTOs.ProfileDto?> GetProfileAsync(BitsBlog.Application.DTOs.ProfileQueryDto query, System.Threading.CancellationToken ct = default);

        // Admin management
        Task<IReadOnlyList<BitsBlog.Application.DTOs.UserDto>> ListUsersAsync(BitsBlog.Application.DTOs.UserQueryDto query, System.Threading.CancellationToken ct = default);
        Task<BitsBlog.Application.DTOs.UserDto?> GetUserByIdAsync(int id, System.Threading.CancellationToken ct = default);
        Task<int> CountUsersAsync(BitsBlog.Application.DTOs.UserQueryDto query, System.Threading.CancellationToken ct = default);
        Task<bool> SetRoleAsync(int id, string role, System.Threading.CancellationToken ct = default);
        Task<bool> DeleteUserAsync(int id, System.Threading.CancellationToken ct = default);

        // Profile management
        Task<BitsBlog.Application.DTOs.ResultDto> UpdateDisplayNameAsync(BitsBlog.Application.DTOs.UpdateProfileDto dto, System.Threading.CancellationToken ct = default);
        Task<BitsBlog.Application.DTOs.ResultDto> ChangePasswordAsync(BitsBlog.Application.DTOs.ChangePasswordDto dto, System.Threading.CancellationToken ct = default);
    }
}
