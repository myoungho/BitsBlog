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

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? q = null, string? sort = null)
        {
            (page, pageSize) = BitsBlog.Web.Services.PagingUtils.Normalize(page, pageSize);
            var url = $"posts?page={page}&pageSize={pageSize}" +
                      (string.IsNullOrWhiteSpace(q) ? string.Empty : $"&q={Uri.EscapeDataString(q)}") +
                      (string.IsNullOrWhiteSpace(sort) ? string.Empty : $"&sort={Uri.EscapeDataString(sort)}");
            var res = await Api().GetAsync(url);
            res.EnsureSuccessStatusCode();
            var items = await res.Content.ReadFromJsonAsync<IReadOnlyList<PostVm>>() ?? Array.Empty<PostVm>();
            var total = BitsBlog.Web.Services.PagingUtils.ParseTotalCount(res);
            var vm = new BitsBlog.Web.Models.PagedResult<PostVm>(items, page, pageSize, total);
            ViewData["q"] = q; ViewData["sort"] = sort;
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
