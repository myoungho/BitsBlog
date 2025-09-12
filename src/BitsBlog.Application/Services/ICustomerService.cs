using System;
using System.Threading.Tasks;
using BitsBlog.Domain.Entities;

namespace BitsBlog.Application.Services
{
    public interface ICustomerService
    {
        Task<(bool ok, string? error)> EnsureAdminAsync(string email, string password, string displayName);

        Task<(bool ok, string? error, Customer? customer)> RegisterAsync(string email, string password, string? displayName);

        Task<(bool ok, string? error, Customer? customer)> LoginAsync(string email, string password);

        Task<(string LoginId, string DisplayName, string Role, DateTime Created)?> GetProfileAsync(string loginId);

        // Admin management
        Task<IReadOnlyList<BitsBlog.Application.DTOs.UserDto>> ListUsersAsync(int skip = 0, int take = 100);
        Task<BitsBlog.Application.DTOs.UserDto?> GetUserByIdAsync(int id);
        Task<int> CountUsersAsync();
        Task<bool> SetRoleAsync(int id, string role);
        Task<bool> DeleteUserAsync(int id);

        // Profile management
        Task<(bool ok, string? error)> UpdateDisplayNameAsync(string loginId, string displayName);
        Task<(bool ok, string? error)> ChangePasswordAsync(string loginId, string currentPassword, string newPassword);
    }
}
