using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BitsBlog.Application.DTO;

namespace BitsBlog.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        public AccountController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient Api() => _clientFactory.CreateClient("api");

        private static Microsoft.AspNetCore.Http.CookieOptions JwtCookieOptions(System.DateTime expires)
            => new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = false, // keep existing behavior
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = expires
            };

        private void SetJwtCookie(AuthResponse auth)
        {
            Response.Cookies.Append("jwt", auth.AccessToken, JwtCookieOptions(auth.Expires));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        public record AuthResponse(string AccessToken, System.DateTime Expires, string Role, string DisplayName);

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var client = Api();
            var res = await client.PostAsJsonAsync("auth/login", new LoginDto { Email = email, Password = password });
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
            SetJwtCookie(auth);
            // If Admin, redirect to Admin dashboard
            if (string.Equals(auth.Role, "Admin", System.StringComparison.OrdinalIgnoreCase))
                return Redirect("/Admin");
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string email, string password, string displayName)
        {
            var client = Api();
            var res = await client.PostAsJsonAsync("auth/register", new RegisterDto { Email = email, Password = password, DisplayName = displayName });
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
            SetJwtCookie(auth);
            if (string.Equals(auth.Role, "Admin", System.StringComparison.OrdinalIgnoreCase))
                return Redirect("/Admin");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var client = Api();
            var meResp = await client.GetAsync("auth/me");
            if (!meResp.IsSuccessStatusCode) return RedirectToAction("Login");
            var json = await meResp.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonObject>();
            var loginId = (string?)json?["loginId"] ?? (string?)json?["LoginId"] ?? string.Empty;
            var display = (string?)json?["displayName"] ?? (string?)json?["DisplayName"] ?? loginId;
            var role = (string?)json?["role"] ?? (string?)json?["Role"] ?? "User";
            var created = (System.DateTime?)json?["created"] ?? default;
            var vm = new ProfileViewModel(loginId, display, role, created);
            return View(vm);
        }

        public record ProfileViewModel(string LoginId, string DisplayName, string Role, System.DateTime? Created);

        public record UpdateProfileRequest(string DisplayName);

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Profile(string displayName)
        {
            var client = Api();
            var res = await client.PutAsJsonAsync("auth/profile", new UpdateProfileDto { DisplayName = displayName });
            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to update profile.";
                return RedirectToAction(nameof(Profile));
            }
            var auth = await res.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth is not null)
            {
                SetJwtCookie(auth);
            }
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var client = Api();
            var res = await client.PutAsJsonAsync("auth/password", new ChangePasswordDto { CurrentPassword = currentPassword, NewPassword = newPassword });
            if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                TempData["PasswordError"] = "Current password is incorrect.";
            }
            else if (!res.IsSuccessStatusCode)
            {
                TempData["PasswordError"] = "Failed to change password.";
            }
            else
            {
                TempData["PasswordSuccess"] = "Password changed successfully.";
            }
            return RedirectToAction(nameof(Profile));
        }
    }
}
