using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReCenterHub.Models;
using ReCenterHub.Services;
using ReCenterHub.ViewModels;

namespace ReCenterHub.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IndividualBookingService _individualBookingService;
        private readonly WorkshopBookingService _workshopBookingService;

        public AdminController(
            IndividualBookingService individualBookingService,
            WorkshopBookingService workshopBookingService)
        {
            _individualBookingService = individualBookingService;
            _workshopBookingService = workshopBookingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var individualBookings = await _individualBookingService.GetAllIndividualBookingsAsync();

                var workshopBookings = await _workshopBookingService.GetAllWorkshopBookingsAsync();

                var model = new AdminDashboardViewModel
                {
                    IndividualBookings = individualBookings ?? new List<IndividualBooking>(),
                    WorkshopBookings = workshopBookings ?? new List<WorkshopBooking>()
                };

                model.NewBookingCount =
                    model.IndividualBookings.Count(b =>
                        string.Equals(
                            b.Status,
                            BookingStatuses.Pending,
                            StringComparison.OrdinalIgnoreCase))
                    +
                    model.WorkshopBookings.Count(b =>
                        string.Equals(
                            b.Status,
                            BookingStatuses.Pending,
                            StringComparison.OrdinalIgnoreCase));

                if (individualBookings == null || workshopBookings == null)
                {
                    TempData["Error"] = "One or more booking lists could not be loaded from the API.";
                }

                return View(model);
            }
            catch (ApiUnauthorizedException)
            {
                return RedirectToAction("Login","Account",new{returnUrl = Url.Action(nameof(Index),"Admin")});
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "The API could not be reached. Make sure the API is running.";

                return View(new AdminDashboardViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> PatientHistory(string? firstName, string? surname)
        {
            try
            {
                var bookings = await _individualBookingService.GetAllIndividualBookingsAsync();

                if (bookings == null)
                {
                    TempData["Error"] = "Booking list could not be loaded. Please try again later.";
                    bookings = new List<IndividualBooking>();
                }

                ViewData["firstName"] = firstName;
                ViewData["surname"] = surname;
                var query = bookings.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(firstName))
                {
                    var term = firstName.Trim();
                    query = query.Where(b => b.FirstName?.Contains(term, StringComparison.OrdinalIgnoreCase) == true);
                }

                if (!string.IsNullOrWhiteSpace(surname))
                {
                    var term = surname.Trim();
                    query = query.Where(b =>b.Surname?.Contains(term, StringComparison.OrdinalIgnoreCase) == true);
                }
                var sortedBookings = query.OrderByDescending(b => b.Date_and_Time).ToList();
                return View(sortedBookings);
            }
            catch (ApiUnauthorizedException)
            {
                return RedirectToAction("Login", "Account",
                    new { returnUrl = Url.Action(nameof(PatientHistory), "Admin") });
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "The API could not be reached. Please try again later.";
                return View(new List<IndividualBooking>());
            }
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetWorkshopStatus(
            int id,
            string status)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            if (!BookingStatuses.IsValid(status))
            {
                TempData["Error"] =
                    "The selected workshop booking status is not valid.";

                return RedirectToAction(nameof(Index));
            }

            var booking =
                await _workshopBookingService
                    .GetWorkshopBookingByIdAsync(id);

            if (booking == null)
            {
                TempData["Error"] =
                    "The workshop booking could not be found.";

                return RedirectToAction(nameof(Index));
            }

            booking.Status = status;

            var updated =
                await _workshopBookingService.UpdateAsync(booking);

            TempData[updated != null ? "Notification" : "Error"] =
                updated != null
                    ? "The workshop booking status was updated successfully."
                    : "The workshop booking status could not be updated.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteIndividualBooking(int? id)
        {
            var booking = await _individualBookingService.GetIndividualBookingByIdAsync(id.Value);

            if (booking == null)
            {
                return NotFound();
            }
            var deleted = await _individualBookingService.Delete(booking);

            TempData[deleted ? "Notification" : "Error"] = deleted
                ? "The individual booking was deleted."
                : "The individual booking could not be deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWorkshopBooking(int? id)
        {
            var booking = await _workshopBookingService.GetWorkshopBookingByIdAsync(id.Value);

            if (booking == null)
            {
                return NotFound();
            }
            var deleted = await _workshopBookingService.DeleteAsync(id);

            TempData[deleted ? "Notification" : "Error"] = deleted
                ? "The workshop booking was deleted."
                : "The workshop booking could not be deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditIndividualBooking(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var booking = await _individualBookingService.GetIndividualBookingByIdAsync(id.Value);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIndividualBooking( int? id,
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
            var updated = await _individualBookingService.UpdateAsync(ib);

            if (updated == null)
            {
                TempData["Error"] = "The booking could not be updated.";
                return View(ib);
            }

            TempData["Notification"] = "The booking was updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWorkshopBooking(
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

            var updated = await _workshopBookingService.UpdateAsync(wb);

            if (updated == null)
            {
                TempData["Error"] = "The booking could not be updated.";
                return View(wb);
            }

            TempData["Notification"] = "The booking was updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> AddSessionNotes(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var booking = await _individualBookingService.GetIndividualBookingByIdAsync(id.Value);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

       
   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSessionNotes(int id,string sessionNotes)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            var booking =
                await _individualBookingService
                    .GetIndividualBookingByIdAsync(id);

            if (booking == null)
            {
                TempData["Error"] =
                    "The individual booking could not be found.";

                return RedirectToAction(nameof(Index));
            }

            booking.SessionNotes = sessionNotes;

            var updated =
                await _individualBookingService.UpdateAsync(booking);

            TempData[updated != null ? "Notification" : "Error"] =
                updated != null
                    ? "The individual booking session notes were updated successfully."
                    : "The individual booking session notes could not be updated.";

            return RedirectToAction(nameof(PatientHistory));
        }

        [HttpGet]
        public async Task<IActionResult> ViewSessionNotes(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var booking = await _individualBookingService.GetIndividualBookingByIdAsync(id.Value);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> EditSessionNotes(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var booking = await _individualBookingService.GetIndividualBookingByIdAsync(id.Value);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSessionNotes(int id, string sessionNotes)
        {
            if (id <= 0)
            {
                return NotFound();
            }
            var booking =
                await _individualBookingService
                    .GetIndividualBookingByIdAsync(id);

            if (booking == null)
            {
                TempData["Error"] =
                    "The individual booking could not be found.";

                return RedirectToAction(nameof(Index));
            }

            booking.SessionNotes = sessionNotes;

            var updated =
                await _individualBookingService.UpdateAsync(booking);

            TempData[updated != null ? "Notification" : "Error"] =
                updated != null
                    ? "The individual booking session notes were updated successfully."
                    : "The individual booking session notes could not be updated.";

            return RedirectToAction(nameof(PatientHistory));
        }




    }
}