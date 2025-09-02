using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BitsBlog.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BitsBlog.MvcClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public HomeController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _clientFactory.CreateClient("api");
            var posts = await client.GetFromJsonAsync<IEnumerable<PostDto>>("posts");
            return View(posts);
        }
    }
}
