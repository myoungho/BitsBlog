using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;
using BitsBlog.Web.Models;
using Microsoft.AspNetCore.Mvc;

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
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new System.Uri("http://localhost/api/");
            }
            var posts = await client.GetFromJsonAsync<IEnumerable<PostDto>>("posts");
            return View(posts);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var client = _clientFactory.CreateClient("api");
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new System.Uri("http://localhost/api/");
            }
            await client.PostAsJsonAsync("posts", new { model.Title, model.Content });
            return RedirectToAction("Index", "Home");
        }
    }
}
