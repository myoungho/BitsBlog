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

        public async Task<IActionResult> Index()
        {
            var posts = await Api().GetFromJsonAsync<IEnumerable<PostVm>>("posts");
            return View(posts ?? Enumerable.Empty<PostVm>());
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

