using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly IApiService _api; public OrganiserController(IApiService api) { _api = api; }
        private bool IsOrg() => HttpContext.Session.GetString("UserRole") == "Organiser";
        public async Task<IActionResult> Dashboard() { if (!IsOrg()) return RedirectToAction("Login", "Account"); return View(await _api.GetAsync<List<EventDto>>("/api/events") ?? new()); }
        public async Task<IActionResult> Events() { if (!IsOrg()) return RedirectToAction("Login", "Account"); return View(await _api.GetAsync<List<EventDto>>("/api/events") ?? new()); }
        public IActionResult CreateEvent() => IsOrg() ? View() : RedirectToAction("Login", "Account");
        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventDto model, IFormFile bannerFile)
        {
            if (!IsOrg()) return RedirectToAction("Login", "Account");
            var res = await _api.CreateEventAsync(model);
            if (res.IsSuccess && res.Data != null)
            {
                if (bannerFile != null) await _api.PostFileAsync($"/api/events/{res.Data.EventID}/banner", bannerFile);
                TempData["SuccessMessage"] = "Event created successfully!"; return RedirectToAction("Events");
            }
            return View(model);
        }
        public async Task<IActionResult> Categories(int eventId) { if (!IsOrg()) return RedirectToAction("Login", "Account"); ViewBag.EventId = eventId; return View(await _api.GetCategoriesAsync(eventId) ?? new()); }
        [HttpPost]
        public async Task<IActionResult> CreateCategory(int eventId, string categoryName, string description)
        {
            await _api.PostAsync($"/api/events/{eventId}/categories", new CategoryDto { EventID = eventId, CategoryName = categoryName, Description = description });
            return RedirectToAction("Categories", new { eventId });
        }
        public async Task<IActionResult> ManageResults(int eventId) { if (!IsOrg()) return RedirectToAction("Login", "Account"); ViewBag.EventId = eventId; return View(await _api.GetEventEnrollmentsAsync(eventId) ?? new()); }
        [HttpPost]
        public async Task<IActionResult> CaptureResult(int eventId, int enrollmentId, TimeSpan finishTime, int position)
        {
            await _api.CaptureResultAsync(enrollmentId, finishTime, position); return RedirectToAction("ManageResults", new { eventId });
        }
    }
}