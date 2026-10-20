using Api.Data;
using Api.Models;
using Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndividualBookingController : ControllerBase
    {
        private readonly AuthDbContext _authDbContext;

        public IndividualBookingController(
            AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        private string? CurrentUserId =>
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        private bool CanAccess(
            IndividualBooking booking) =>
            User.IsInRole("Admin") ||
            booking.UserId == CurrentUserId;

        // ADMIN: GET ALL INDIVIDUAL BOOKINGS
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllBookings()
        {
            var bookings = _authDbContext
                .IndividualBooking
                .OrderBy(b => b.Date_and_Time)
                .ToList();

            return Ok(bookings);
        }

        // CLIENT: GET OWN BOOKINGS
        [HttpGet("client-bookings")]
        [Authorize(Roles = "Client")]
        public IActionResult GetClientBookings()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var bookings = _authDbContext
                .IndividualBooking
                .Where(b => b.UserId == userId)
                .OrderBy(b => b.Date_and_Time)
                .ToList();

            return Ok(bookings);
        }

        // CLIENT OR ADMIN: GET ONE BOOKING
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Client,Admin")]
        public IActionResult GetBookingById(int id)
        {
            var booking =
                _authDbContext.IndividualBooking.Find(id);

            if (booking is null)
            {
                return NotFound();
            }

            if (!CanAccess(booking))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden);
            }

            return Ok(booking);
        }

        // CLIENT: CREATE BOOKING
        [HttpPost]
        [Authorize(Roles = "Client")]
        public IActionResult AddIndividualBookings(
            AddIndividualBookingDTO individualBookingentity)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var individualBooking =
                new IndividualBooking
                {
                    UserId = userId,
                    FirstName =
                        individualBookingentity.FirstName,
                    Surname =
                        individualBookingentity.Surname,
                    Category =
                        individualBookingentity.Category,
                    PhoneNumber =
                        individualBookingentity.PhoneNumber,
                    EmailAddress =
                        individualBookingentity.EmailAddress,
                    Date_and_Time =
                        individualBookingentity.Date_and_Time,
                    OptionalNotes =
                        individualBookingentity.OptionalNotes,

                    Status = "Scheduled",
                    SessionNotes = individualBookingentity.SessionNotes
                };

            _authDbContext
                .IndividualBooking
                .Add(individualBooking);

            _authDbContext.SaveChanges();

            return Ok(individualBooking);
        }

        // CLIENT OR ADMIN: UPDATE BOOKING
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Client,Admin")]
        public IActionResult UpdateBooking(
            int id,
            UpdateIndividualBookingDTO individualBookingentity)
        {
            var individualBooking =
                _authDbContext
                    .IndividualBooking
                    .Find(id);

            if (individualBooking is null)
            {
                return NotFound();
            }

            if (!CanAccess(individualBooking))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden);
            }

            individualBooking.FirstName =
                individualBookingentity.FirstName;

            individualBooking.Surname =
                individualBookingentity.Surname;

            individualBooking.EmailAddress =
                individualBookingentity.EmailAddress;

            individualBooking.Category =
                individualBookingentity.Category;

            individualBooking.PhoneNumber =
                individualBookingentity.PhoneNumber;

            individualBooking.Date_and_Time =
                individualBookingentity.Date_and_Time;

            individualBooking.OptionalNotes =
                individualBookingentity.OptionalNotes;

            
            // ONLY ADMIN CAN CHANGE STATUS
            if (User.IsInRole("Admin") && !string.IsNullOrWhiteSpace( individualBookingentity.Status) || !string.IsNullOrWhiteSpace( individualBookingentity.SessionNotes))
            {
                individualBooking.Status =  individualBookingentity.Status;
                individualBooking.SessionNotes = individualBookingentity.SessionNotes;
            }

            _authDbContext.SaveChanges();

            return Ok(individualBooking);
        }

        // ADMIN: DELETE BOOKING
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Client")]
        public IActionResult DeleteBooking(int id)
        {
            var individualBooking =
                _authDbContext
                    .IndividualBooking
                    .Find(id);

            if (individualBooking is null)
            {
                return NotFound();
            }

            if (!CanAccess(individualBooking))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden);
            }

            _authDbContext
                .IndividualBooking
                .Remove(individualBooking);

            _authDbContext.SaveChanges();

            return Ok();
        }
    }
}