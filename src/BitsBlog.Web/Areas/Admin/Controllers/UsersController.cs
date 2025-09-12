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
        public async Task<IActionResult> Delete(int id, string? mode)
        {
            var client = Api();
            var m = string.IsNullOrWhiteSpace(mode) ? "anonymize" : mode;
            var res = await client.DeleteAsync($"users/{id}?mode={Uri.EscapeDataString(m!)}");
            if (!res.IsSuccessStatusCode) TempData["Error"] = "Failed to delete user.";
            return RedirectToAction(nameof(Index));
        }
    }
}
