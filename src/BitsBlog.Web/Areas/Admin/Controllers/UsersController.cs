using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public record SetRoleRequest(string Role);

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            (page, pageSize) = BitsBlog.Web.Services.PagingUtils.Normalize(page, pageSize);
            var client = Api();
            var res = await client.GetAsync($"users?page={page}&pageSize={pageSize}");
            res.EnsureSuccessStatusCode();
            var items = await res.Content.ReadFromJsonAsync<IReadOnlyList<UserVm>>() ?? Array.Empty<UserVm>();
            var total = BitsBlog.Web.Services.PagingUtils.ParseTotalCount(res);
            var vm = new BitsBlog.Web.Models.PagedResult<UserVm>(items, page, pageSize, total);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Promote(int id)
        {
            var client = Api();
            var res = await client.PutAsJsonAsync($"users/{id}/role", new SetRoleRequest("Admin"));
            if (!res.IsSuccessStatusCode) TempData["Error"] = "역할 변경 실패";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Demote(int id)
        {
            var client = Api();
            var res = await client.PutAsJsonAsync($"users/{id}/role", new SetRoleRequest("User"));
            if (!res.IsSuccessStatusCode) TempData["Error"] = "역할 변경 실패";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = Api();
            var res = await client.DeleteAsync($"users/{id}");
            if (!res.IsSuccessStatusCode) TempData["Error"] = "삭제 실패";
            return RedirectToAction(nameof(Index));
        }
    }
}
