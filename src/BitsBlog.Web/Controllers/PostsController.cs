using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;
using BitsBlog.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;

namespace BitsBlog.Web.Controllers
{
    public class PostsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public PostsController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? q = null, string? sort = null)
        {
            (page, pageSize) = BitsBlog.Web.Services.PagingUtils.Normalize(page, pageSize);
            var client = _clientFactory.CreateClient("api");
            var url = $"posts?page={page}&pageSize={pageSize}" +
                      (string.IsNullOrWhiteSpace(q) ? string.Empty : $"&q={Uri.EscapeDataString(q)}") +
                      (string.IsNullOrWhiteSpace(sort) ? string.Empty : $"&sort={Uri.EscapeDataString(sort)}");
            var res = await client.GetAsync(url);
            res.EnsureSuccessStatusCode();
            var items = await res.Content.ReadFromJsonAsync<IReadOnlyList<PostDto>>() ?? Array.Empty<PostDto>();
            var total = BitsBlog.Web.Services.PagingUtils.ParseTotalCount(res);
            var vm = new BitsBlog.Web.Models.PagedResult<PostDto>(items, page, pageSize, total);
            ViewData["q"] = q;
            ViewData["sort"] = sort;
            return View(vm);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var client = _clientFactory.CreateClient("api");
            await client.PostAsJsonAsync("posts", new BitsBlog.Application.DTOs.PostCreateDto { Title = model.Title, Content = model.Content });
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _clientFactory.CreateClient("api");
            var post = await client.GetFromJsonAsync<PostDto>($"posts/{id}");
            if (post is null) return NotFound();
            var comments = await client.GetFromJsonAsync<IEnumerable<CommentDto>>($"posts/{id}/comments");
            var vm = new PostDetailsViewModel { Post = post, Comments = comments?.ToList() ?? new List<CommentDto>() };
            var token = HttpContext.Request.Cookies["jwt"];
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var meResp = await client.GetAsync("auth/me");
                    if (meResp.IsSuccessStatusCode)
                    {
                        var json = await meResp.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonObject>();
                        vm.CurrentUserLoginId = (string?)json?["loginId"] ?? (string?)json?["LoginId"];
                        var roleVal = (string?)json?["role"] ?? (string?)json?["Role"];
                        vm.IsAdmin = string.Equals(roleVal, "Admin", System.StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch { }
            }
            return View(vm);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _clientFactory.CreateClient("api");
            var post = await client.GetFromJsonAsync<PostDto>($"posts/{id}");
            if (post is null) return NotFound();
            var vm = new EditPostViewModel { Id = post.Id, Title = post.Title, Content = post.Content };
            return View(vm);
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(EditPostViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var client = _clientFactory.CreateClient("api");
            var res = await client.PutAsJsonAsync($"posts/{model.Id}", new BitsBlog.Application.DTOs.PostUpdateDto { Id = model.Id, Title = model.Title, Content = model.Content });
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound();
            res.EnsureSuccessStatusCode();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _clientFactory.CreateClient("api");
            var res = await client.DeleteAsync($"posts/{id}");
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound();
            res.EnsureSuccessStatusCode();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            var token3 = HttpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token3))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Posts/Details/{postId}" });
            }
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction("Details", new { id = postId });
            }
            var client = _clientFactory.CreateClient("api");
            var res = await client.PostAsJsonAsync($"posts/{postId}/comments", new BitsBlog.Application.DTOs.CommentCreateDto { PostId = postId, Content = content });
            // 성공/실패 무관히 상세로 복귀
            return RedirectToAction("Details", new { id = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> EditComment(int postId, int commentId, string content)
        {
            var token = HttpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Posts/Details/{postId}" });
            var client = _clientFactory.CreateClient("api");
            await client.PutAsJsonAsync($"posts/{postId}/comments/{commentId}", new BitsBlog.Application.DTOs.CommentUpdateDto { CommentId = commentId, Content = content });
            return RedirectToAction("Details", new { id = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int postId, int commentId)
        {
            var token = HttpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Posts/Details/{postId}" });
            var client = _clientFactory.CreateClient("api");
            await client.DeleteAsync($"posts/{postId}/comments/{commentId}");
            return RedirectToAction("Details", new { id = postId });
        }
    }
}
