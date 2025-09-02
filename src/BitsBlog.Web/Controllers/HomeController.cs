using Microsoft.AspNetCore.Mvc;

namespace BitsBlog.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "Posts");
    }
}
