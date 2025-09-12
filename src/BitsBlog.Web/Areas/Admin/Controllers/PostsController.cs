using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BitsBlog.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PostsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public PostsController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public class PostVm
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? AuthorDisplayName { get; set; }
            public System.DateTime Created { get; set; }
        }

        private HttpClient Api() => _clientFactory.CreateClient("api");

        public record Paged<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            if (page < 1) page = 1; if (pageSize < 1) pageSize = 10; if (pageSize > 100) pageSize = 100;
            var res = await Api().GetAsync($"posts?page={page}&pageSize={pageSize}");
            res.EnsureSuccessStatusCode();
            var items = await res.Content.ReadFromJsonAsync<IReadOnlyList<PostVm>>() ?? Array.Empty<PostVm>();
            int total = 0; if (res.Headers.TryGetValues("X-Total-Count", out var vals)) int.TryParse(vals.FirstOrDefault(), out total);
            var vm = new Paged<PostVm>(items, page, pageSize, total);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await Api().DeleteAsync($"posts/{id}");
            if (!res.IsSuccessStatusCode) TempData["Error"] = "게시글 삭제 실패";
            return RedirectToAction(nameof(Index));
        }
    }
}
