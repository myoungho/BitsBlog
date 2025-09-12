using System;
using System.Threading.Tasks;
using BitsBlog.Domain.Entities;

namespace BitsBlog.Application.Services
{
    public interface ICustomerService
    {
        Task<BitsBlog.Application.DTOs.ResultDto> EnsureAdminAsync(string email, string password, string displayName);

        Task<BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>> RegisterAsync(string email, string password, string? displayName);

        Task<BitsBlog.Application.DTOs.ResultDto<BitsBlog.Application.DTOs.AuthUserDto>> LoginAsync(string email, string password);

        Task<BitsBlog.Application.DTOs.ProfileDto?> GetProfileAsync(string loginId);

        // Admin management
        Task<IReadOnlyList<BitsBlog.Application.DTOs.UserDto>> ListUsersAsync(int skip = 0, int take = 100);
        Task<BitsBlog.Application.DTOs.UserDto?> GetUserByIdAsync(int id);
        Task<int> CountUsersAsync();
        Task<bool> SetRoleAsync(int id, string role);
        Task<bool> DeleteUserAsync(int id);

        // Profile management
        Task<BitsBlog.Application.DTOs.ResultDto> UpdateDisplayNameAsync(string loginId, string displayName);
        Task<BitsBlog.Application.DTOs.ResultDto> ChangePasswordAsync(string loginId, string currentPassword, string newPassword);
    }
}
