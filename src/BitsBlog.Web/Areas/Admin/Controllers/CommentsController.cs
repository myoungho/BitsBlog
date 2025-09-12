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
        public async Task<IActionResult> Index(int? postId)
        {
            IEnumerable<CommentVm> comments = Enumerable.Empty<CommentVm>();
            if (postId.HasValue)
            {
                comments = await Api().GetFromJsonAsync<IEnumerable<CommentVm>>($"posts/{postId.Value}/comments")
                           ?? Enumerable.Empty<CommentVm>();
            }
            ViewData["PostId"] = postId;
            return View(comments);
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

