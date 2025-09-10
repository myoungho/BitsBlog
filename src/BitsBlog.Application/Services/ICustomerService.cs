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
    }
}

