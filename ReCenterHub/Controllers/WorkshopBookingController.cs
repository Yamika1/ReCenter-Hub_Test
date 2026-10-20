using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReCenterHub.Models;
using ReCenterHub.Services;
using System.ComponentModel.DataAnnotations;
using static ReCenterHub.Services.ConcreteObserver;


namespace ReCenterHub.Controllers
{
    [Authorize(Roles = "Client,Admin")]
    public class WorkshopBookingController : Controller
    {
        private readonly WorkshopBookingService _wbs;
        private readonly Notifier _notifier;
        public int newBookingCount = 0;


        public WorkshopBookingController(WorkshopBookingService wbs, Notifier notifier)
        {
            _wbs = wbs;
            _notifier = notifier;
        }
        private static bool Matches(string? value, string search) =>
          value?.Contains(search, StringComparison.OrdinalIgnoreCase) == true;

       
        public async Task<IActionResult> Index(string? topic, string? instituitionName)
        {
            var isAdmin = User.IsInRole("Admin");

            var loaded = isAdmin
                ? await _wbs.GetAllWorkshopBookingsAsync()
                : await _wbs.GetClientWorkshopsAsync();

            if (loaded == null)
            {
                TempData["Error"] = "The workshop bookings could not be loaded.";
            }

            var bookings = loaded ?? new List<WorkshopBooking>();

            if (isAdmin)
            {
                var upcoming = _wbs.UpcomingSessions(bookings).Count;
                ViewData["AdminNotification"] = $"You have {upcoming} upcoming workshop booking/s.";
            }

            if (!string.IsNullOrWhiteSpace(instituitionName))
                bookings = bookings.Where(b => Matches(b.InstitutionName, instituitionName)).ToList();

            if (!string.IsNullOrWhiteSpace(topic))
                bookings = bookings.Where(b => Matches(b.Topic, topic)).ToList();

            return View(bookings);
        }
        

           
        [HttpGet]
        public IActionResult Create()
        {
            return View(new WorkshopBooking
            {
                Date_and_Time = DateTime.Today.AddDays(7).AddHours(9)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
           [Bind("InstitutionName,TargetAudience,Topic,PhoneNumber,EmailAddress,Date_and_Time,OptionalNotes")] WorkshopBooking wb)
        {
            if (!ModelState.IsValid)
            {
                return View(wb);
            }

            var created = await _wbs.CreateAsync(wb);

            if (created == null)
            {
                TempData["Error"] = "Your workshop request could not be saved. Please try again.";
                return View(wb);
            }

            return RedirectToAction("Index", "SuccessTab");
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _wbs.GetWorkshopBookingByIdAsync(id.Value);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            int? id,
            [Bind("InstitutionName,TargetAudience,Topic,PhoneNumber,EmailAddress,Date_and_Time,OptionalNotes,Status")] WorkshopBooking wb)
        {
            if (id == null)
            {
                return NotFound();
            }

            wb.WorkshopBookingID = id.Value;

            if (!ModelState.IsValid)
            {
                return View(wb);
            }

            var updated = await _wbs.UpdateAsync(wb);

            if (updated == null)
            {
                TempData["Error"] = "The workshop booking could not be updated.";
                return View(wb);
            }

            TempData["Notification"] = "The workshop booking was updated.";
            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deleted = await _wbs.DeleteAsync(id);

            TempData[deleted ? "Notification" : "Error"] = deleted
                ? "The workshop booking was deleted."
                : "The workshop booking could not be deleted.";

            return RedirectToAction(nameof(Index));
        }
    }
}




