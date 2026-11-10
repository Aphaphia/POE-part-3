using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IApiService _api; public ParticipantController(IApiService api) { _api = api; }
        private bool IsPart() => HttpContext.Session.GetString("UserRole") == "Participant";
        public async Task<IActionResult> Home() { if (!IsPart()) return RedirectToAction("Login", "Account"); return View(await _api.GetAsync<List<EventDto>>("/api/events") ?? new()); }
        public async Task<IActionResult> EventDetails(int id)
        {
            if (!IsPart()) return RedirectToAction("Login", "Account");
            var ev = await _api.GetAsync<EventDto>($"/api/events/{id}");
            if (ev == null) { TempData["ErrorMessage"] = "Event not found."; return RedirectToAction("Home"); }
            ViewBag.Categories = await _api.GetCategoriesAsync(id) ?? new(); return View(ev);
        }
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            if (!IsPart()) return RedirectToAction("Login", "Account");
            var ok = await _api.EnrolAsync(eventId, categoryId);
            if (ok) { TempData["SuccessMessage"] = "Successfully enrolled!"; return RedirectToAction("MyEvents"); }
            TempData["ErrorMessage"] = "Failed to enrol."; return RedirectToAction("EventDetails", new { id = eventId });
        }
        public async Task<IActionResult> MyEvents() { if (!IsPart()) return RedirectToAction("Login", "Account"); return View(await _api.GetMyEnrollmentsAsync() ?? new()); }
        public async Task<IActionResult> MyResults() { if (!IsPart()) return RedirectToAction("Login", "Account"); return View(await _api.GetMyResultsAsync() ?? new()); }
    }
}