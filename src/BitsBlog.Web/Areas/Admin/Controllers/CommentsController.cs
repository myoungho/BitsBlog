using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BitsBlog.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CommentsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public CommentsController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public class CommentVm
        {
            public int Id { get; set; }
            public int PostId { get; set; }
            public string Content { get; set; } = string.Empty;
            public string? AuthorDisplayName { get; set; }
            public System.DateTime Created { get; set; }
        }

        private HttpClient Api() => _clientFactory.CreateClient("api");

        [HttpGet]
        public async Task<IActionResult> Index(int? postId, int page = 1, int pageSize = 10)
        {
            (page, pageSize) = BitsBlog.Web.Services.PagingUtils.Normalize(page, pageSize);
            int total = 0; IReadOnlyList<CommentVm> items = Array.Empty<CommentVm>();
            if (postId.HasValue)
            {
                var res = await Api().GetAsync($"posts/{postId.Value}/comments?page={page}&pageSize={pageSize}");
                res.EnsureSuccessStatusCode();
                items = await res.Content.ReadFromJsonAsync<IReadOnlyList<CommentVm>>() ?? Array.Empty<CommentVm>();
                total = BitsBlog.Web.Services.PagingUtils.ParseTotalCount(res);
            }
            ViewData["PostId"] = postId;
            var vm = new BitsBlog.Web.Models.PagedResult<CommentVm>(items, page, pageSize, total);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int postId, int commentId)
        {
            var res = await Api().DeleteAsync($"posts/{postId}/comments/{commentId}");
            if (!res.IsSuccessStatusCode) TempData["Error"] = "댓글 삭제 실패";
            return RedirectToAction(nameof(Index), new { postId });
        }
    }
}
