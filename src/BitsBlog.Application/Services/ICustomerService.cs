using System;
using System.Threading.Tasks;
using BitsBlog.Domain.Entities;

namespace BitsBlog.Application.Services
{
    public interface ICustomerService
    {
        Task<BitsBlog.Application.DTOs.ResultDto> EnsureAdminAsync(BitsBlog.Application.DTOs.AdminSeedDto dto);

        Task<BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>> RegisterAsync(BitsBlog.Application.DTOs.RegisterDto dto);

        Task<BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>> LoginAsync(BitsBlog.Application.DTOs.LoginDto dto);

        Task<BitsBlog.Application.DTOs.ProfileDto?> GetProfileAsync(BitsBlog.Application.DTOs.ProfileQueryDto query);

        // Admin management
        Task<IReadOnlyList<BitsBlog.Application.DTOs.UserDto>> ListUsersAsync(BitsBlog.Application.DTOs.UserQueryDto query);
        Task<BitsBlog.Application.DTOs.UserDto?> GetUserByIdAsync(int id);
        Task<int> CountUsersAsync(BitsBlog.Application.DTOs.UserQueryDto query);
        Task<bool> SetRoleAsync(int id, string role);
        Task<bool> DeleteUserAsync(int id);

        // Profile management
        Task<BitsBlog.Application.DTOs.ResultDto> UpdateDisplayNameAsync(BitsBlog.Application.DTOs.UpdateProfileDto dto);
        Task<BitsBlog.Application.DTOs.ResultDto> ChangePasswordAsync(BitsBlog.Application.DTOs.ChangePasswordDto dto);
    }
}
