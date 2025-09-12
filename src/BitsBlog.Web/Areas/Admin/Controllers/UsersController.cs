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

        public async Task<IActionResult> Index()
        {
            var client = Api();
            var users = await client.GetFromJsonAsync<IEnumerable<UserVm>>("users");
            return View(users ?? Enumerable.Empty<UserVm>());
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

