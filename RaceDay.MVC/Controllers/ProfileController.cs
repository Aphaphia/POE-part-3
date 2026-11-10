using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IApiService _api; public ProfileController(IApiService api) { _api = api; }
        public async Task<IActionResult> Index()
        {
            var idStr = HttpContext.Session.GetString("UserID"); if (string.IsNullOrEmpty(idStr)) return RedirectToAction("Login", "Account");
            var profile = await _api.GetProfileAsync(int.Parse(idStr));
            return View(profile ?? new UserProfileDto { UserID = int.Parse(idStr) });
        }
        [HttpPost]
        public async Task<IActionResult> Update(UserProfileDto model, IFormFile profilePicture)
        {
            var idStr = HttpContext.Session.GetString("UserID"); if (string.IsNullOrEmpty(idStr)) return RedirectToAction("Login", "Account");
            await _api.UpdateProfileAsync(int.Parse(idStr), model);
            if (profilePicture != null) await _api.UploadProfilePictureAsync(profilePicture);
            TempData["SuccessMessage"] = "Profile updated"; return RedirectToAction("Index");
        }
    }
}