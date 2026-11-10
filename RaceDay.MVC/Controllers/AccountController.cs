using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;
        public AccountController(IApiService apiService) { _apiService = apiService; }
        public IActionResult Login() => View();
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var res = await _apiService.LoginAsync(model);
            if (res.IsSuccess && res.Data != null)
            {
                HttpContext.Session.SetString("JwtToken", res.Data.Token); HttpContext.Session.SetString("UserRole", res.Data.Role);
                HttpContext.Session.SetString("FullName", res.Data.FullName); HttpContext.Session.SetString("UserID", res.Data.UserID.ToString());
                return res.Data.Role == "Organiser" ? RedirectToAction("Dashboard", "Organiser") : RedirectToAction("Home", "Participant");
            }
            ModelState.AddModelError("", "Invalid login"); return View(model);
        }
        public IActionResult Register() => View();
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var res = await _apiService.PostAsync("/api/auth/register", model);
            if (res.IsSuccess) return RedirectToAction("Login");
            ModelState.AddModelError("", "Registration failed"); return View(model);
        }
        [HttpPost] public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Login"); }
    }
}