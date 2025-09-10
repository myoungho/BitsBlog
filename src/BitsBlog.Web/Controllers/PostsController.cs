using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;
using BitsBlog.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace BitsBlog.Web.Controllers
{
    public class PostsController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public PostsController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.CreateClient("api");
            var posts = await client.GetFromJsonAsync<IEnumerable<PostDto>>("posts");
            return View(posts);
        }

        public IActionResult Create()
        {
            var token = HttpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Posts/Create" });
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            var token = HttpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Posts/Create" });
            }
            if (!ModelState.IsValid) return View(model);
            var client = _clientFactory.CreateClient("api");
            await client.PostAsJsonAsync("posts", new { model.Title, model.Content });
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
            return View(vm);
        }

        [HttpGet]
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
        public async Task<IActionResult> AddComment(int postId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction("Details", new { id = postId });
            }
            var client = _clientFactory.CreateClient("api");
            var res = await client.PostAsJsonAsync($"posts/{postId}/comments", new { content });
            if (!res.IsSuccessStatusCode)
            {
                // If unauthorized or other error, just redirect back
                return RedirectToAction("Details", new { id = postId });
            }
            return RedirectToAction("Details", new { id = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPostViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var client = _clientFactory.CreateClient("api");
            var res = await client.PutAsJsonAsync($"posts/{model.Id}", new { model.Title, model.Content });
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound();
            res.EnsureSuccessStatusCode();
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _clientFactory.CreateClient("api");
            var res = await client.DeleteAsync($"posts/{id}");
            if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
                return NotFound();
            res.EnsureSuccessStatusCode();
            return RedirectToAction("Index", "Home");
        }
    }
}
