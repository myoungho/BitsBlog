using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BitsBlog.Application.DTO;

namespace BitsBlog.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public UsersController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient Api() => _clientFactory.CreateClient("api");

        public record UserVm(int Id, string LoginId, string DisplayName, string Role, System.DateTime Created);

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? q = null, string? sort = null)
        {
            (page, pageSize) = BitsBlog.Web.Services.PagingUtils.Normalize(page, pageSize);
            var client = Api();
            var url = $"users?page={page}&pageSize={pageSize}" +
                      (string.IsNullOrWhiteSpace(q) ? string.Empty : $"&q={Uri.EscapeDataString(q)}") +
                      (string.IsNullOrWhiteSpace(sort) ? string.Empty : $"&sort={Uri.EscapeDataString(sort)}");
            var res = await client.GetAsync(url);
            BitsBlog.Application.DTO.Common.PagedResult<UserVm> vm;
            if (!res.IsSuccessStatusCode)
            {
                ViewData["Error"] = "Failed to load users.";
                vm = new BitsBlog.Application.DTO.Common.PagedResult<UserVm>
                {
                    Items = Array.Empty<UserVm>(),
                    Page = page,
                    PageSize = pageSize,
                    Total = 0
                };
            }
            else
            {
                vm = await res.Content.ReadFromJsonAsync<BitsBlog.Application.DTO.Common.PagedResult<UserVm>>()
                     ?? new BitsBlog.Application.DTO.Common.PagedResult<UserVm>
                     {
                         Items = Array.Empty<UserVm>(),
                         Page = page,
                         PageSize = pageSize,
                         Total = 0
                     };
            }
            ViewData["q"] = q; ViewData["sort"] = sort;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Promote(int id)
        {
            var client = Api();
            var res = await client.PutAsJsonAsync($"users/role", new SetUserRoleDto { Id = id, Role = "Admin" });
            if (!res.IsSuccessStatusCode) TempData["Error"] = "Failed to promote user.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Demote(int id)
        {
            var client = Api();
            var res = await client.PutAsJsonAsync($"users/role", new SetUserRoleDto { Id = id, Role = "User" });
            if (!res.IsSuccessStatusCode) TempData["Error"] = "Failed to demote user.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string email, string password, string? displayName, string role = "User")
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Email and password are required.";
                return RedirectToAction(nameof(Index));
            }
            var client = Api();
            // Register user via Auth API
            var regRes = await client.PostAsJsonAsync("auth/register", new RegisterDto { Email = email, Password = password, DisplayName = displayName });
            if (!regRes.IsSuccessStatusCode)
            {
                TempData["Error"] = regRes.StatusCode == System.Net.HttpStatusCode.Conflict ? "Email already registered." : "Failed to create user.";
                return RedirectToAction(nameof(Index));
            }

            // If role is Admin, set role
            if (string.Equals(role, "Admin", System.StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // Find the created user id by listing with query
                    var searchRes = await client.GetAsync($"users?page=1&pageSize=5&q={Uri.EscapeDataString(email)}");
                    if (searchRes.IsSuccessStatusCode)
                    {
                        var list = await searchRes.Content.ReadFromJsonAsync<BitsBlog.Application.DTO.Common.PagedResult<UserVm>>();
                        var created = list?.Items?.FirstOrDefault(u => string.Equals(u.LoginId, email, System.StringComparison.OrdinalIgnoreCase));
                        if (created is not null)
                        {
                            await client.PutAsJsonAsync($"users/role", new SetUserRoleDto { Id = created.Id, Role = "Admin" });
                        }
                    }
                }
                catch { /* ignore role set failure */ }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = Api();
            var res = await client.DeleteAsync($"users/{id}?mode=anonymize");
            if (!res.IsSuccessStatusCode) TempData["Error"] = "Failed to delete user.";
            return RedirectToAction(nameof(Index));
        }
    }
}
