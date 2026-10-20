using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReCenterHub.Models;
using ReCenterHub.Services;
using System.Net.NetworkInformation;
using static ReCenterHub.Services.ConcreteObserver;

namespace ReCenterHub.Controllers
{
    [Authorize(Roles = "Client,Admin")]
    public class IndividualBookingController : Controller
    {
        private readonly IndividualBookingService _ibs;
        private readonly Notifier _notifier;
        public int newBookingCount = 0;

        public IndividualBookingController(IndividualBookingService ibs, Notifier notifier)
        {
            _ibs = ibs;
            _notifier = notifier;
        }

        private static bool Matches(string? value, string search) =>
            value?.Contains(search, StringComparison.OrdinalIgnoreCase) == true;

        public async Task<IActionResult> Index(string? category, string? firstName, string? surname)
        {
            var isAdmin = User.IsInRole("Admin");

           
            var loaded = isAdmin
                ? await _ibs.GetAllIndividualBookingsAsync()
                : await _ibs.GetClientBookingsAsync();

            if (loaded == null)
            {
                TempData["Error"] = "The bookings could not be loaded.";
            }

            var bookings = loaded ?? new List<IndividualBooking>();
            var upcoming = _ibs.UpcomingSessions(bookings).Count;
            ViewData["UpcomingCount"] = upcoming;

            if (isAdmin)
            {       
                ViewData["AdminNotification"] = $"You have {upcoming} upcoming individual booking/s.";
            }

            if (!string.IsNullOrWhiteSpace(firstName))
                bookings = bookings.Where(b => Matches(b.FirstName, firstName)).ToList();

            if (!string.IsNullOrWhiteSpace(surname))
                bookings = bookings.Where(b => Matches(b.Surname, surname)).ToList();

            if (!string.IsNullOrWhiteSpace(category))
                bookings = bookings.Where(b => Matches(b.Category, category)).ToList();

            return View(bookings);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new IndividualBooking
            {
                Date_and_Time = DateTime.Today.AddDays(1).AddHours(9)
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName, Surname,Category,PhoneNumber,EmailAddress,Date_and_Time,OptionalNotes")] IndividualBooking ib)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var admin = new Notification(newBookingCount);
                    _notifier.Subscribe(admin);

                    await _ibs.CreateAsync(ib);

                    newBookingCount++;
                    _notifier.Notify(newBookingCount);
                    string notificationMessage = " You now have " + newBookingCount + " new booking/s.";
                    TempData["Notification"] = notificationMessage;

                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    TempData["Error"] = ex.Message;
                }
            }

            return View(ib);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _ibs.GetIndividualBookingByIdAsync(id.Value);

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
           [Bind("FirstName,Surname,Category,PhoneNumber,EmailAddress,Date_and_Time,OptionalNotes,Status")] IndividualBooking ib)
        {
            if (id == null)
            {
                return NotFound();
            }

          
            ib.IndividualBookingID = id.Value;

            if (!ModelState.IsValid)
            {
                return View(ib);
            }

            var updated = await _ibs.UpdateAsync(ib);

            if (updated == null)
            {
                TempData["Error"] = "The booking could not be updated.";
                return View(ib);
            }

            TempData["Notification"] = "The booking was updated.";
            return RedirectToAction(nameof(Index));
        }
        

        
        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            var booking = await _ibs.GetIndividualBookingByIdAsync(id.Value);

            if (booking == null)
            {
                return NotFound();
            }
            var deleted = await _ibs.Delete(booking);

            TempData[deleted ? "Notification" : "Error"] = deleted
                ? "The individual booking was deleted."
                : "The individual booking could not be deleted.";
            return RedirectToAction(nameof(Index));
        }


    }
}





