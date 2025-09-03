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

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var client = _clientFactory.CreateClient("api");
            await client.PostAsJsonAsync("posts", new { model.Title, model.Content });
            return RedirectToAction("Index", "Home");
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
