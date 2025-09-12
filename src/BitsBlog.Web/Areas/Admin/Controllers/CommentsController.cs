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
        public async Task<IActionResult> Index(int? postId, int page = 1, int pageSize = 10, string? q = null, string? sort = null)
        {
            (page, pageSize) = BitsBlog.Web.Services.PagingUtils.Normalize(page, pageSize);
            int total = 0; IReadOnlyList<CommentVm> items = Array.Empty<CommentVm>();
            var url = $"comments?page={page}&pageSize={pageSize}" +
                      (postId.HasValue ? $"&postId={postId.Value}" : string.Empty) +
                      (string.IsNullOrWhiteSpace(q) ? string.Empty : $"&q={Uri.EscapeDataString(q)}") +
                      (string.IsNullOrWhiteSpace(sort) ? string.Empty : $"&sort={Uri.EscapeDataString(sort)}");
            var res = await Api().GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                ViewData["Error"] = "Failed to load comments.";
                items = Array.Empty<CommentVm>();
                total = 0;
            }
            else
            {
                var paged = await res.Content.ReadFromJsonAsync<BitsBlog.Application.DTO.Common.PagedResult<CommentVm>>()
                            ?? new BitsBlog.Application.DTO.Common.PagedResult<CommentVm>
                            {
                                Items = Array.Empty<CommentVm>(),
                                Page = page,
                                PageSize = pageSize,
                                Total = 0
                            };
                items = paged.Items;
                total = paged.Total;
            }
            ViewData["PostId"] = postId;
            var vm = new BitsBlog.Application.DTO.Common.PagedResult<CommentVm>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                Total = total
            };
            ViewData["q"] = q; ViewData["sort"] = sort;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int postId, int commentId)
        {
            var res = await Api().DeleteAsync($"comments/{commentId}");
            if (!res.IsSuccessStatusCode) TempData["Error"] = "Failed to delete comment.";
            return RedirectToAction(nameof(Index), new { postId });
        }
    }
}

