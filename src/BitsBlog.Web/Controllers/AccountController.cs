using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BitsBlog.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public AccountController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        public record LoginRequest(string Email, string Password);
        public record AuthResponse(string AccessToken, System.DateTime Expires, string Role, string DisplayName);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var client = _clientFactory.CreateClient("api");
            var res = await client.PostAsJsonAsync("auth/login", new LoginRequest(email, password));
            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View();
            }
            var auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth is null)
            {
                ModelState.AddModelError(string.Empty, "Login failed.");
                return View();
            }
            Response.Cookies.Append("jwt", auth.AccessToken, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = auth.Expires
            });
            // No additional cookie: navbar reads display name from JWT
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        public record RegisterRequest(string Email, string Password, string? DisplayName);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string email, string password, string displayName)
        {
            var client = _clientFactory.CreateClient("api");
            var res = await client.PostAsJsonAsync("auth/register", new RegisterRequest(email, password, displayName));
            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Registration failed (email might be taken).");
                return View();
            }
            var auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth is null)
            {
                ModelState.AddModelError(string.Empty, "Registration failed.");
                return View();
            }
            Response.Cookies.Append("jwt", auth.AccessToken, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = auth.Expires
            });
            // No additional cookie: navbar reads display name from JWT
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var jwt = Request.Cookies["jwt"];
            var display = BitsBlog.Web.Services.JwtReader.TryGetDisplayName(jwt) ?? "User";
            ViewData["DisplayName"] = display;
            return View();
        }
    }
}
