using Api.Data;
using Api.Models;
using Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkshopBookingController : ControllerBase
    {
        private readonly AuthDbContext _authDbContext;

        public WorkshopBookingController(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }
        private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        private bool CanAccess(WorkshopBooking booking) =>
            User.IsInRole("Admin") || booking.UserId == CurrentUserId;

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAllWorkshops()
        {
            return Ok(_authDbContext.WorkshopBooking.ToList());
        }

        [HttpGet]
        [Route("{id:int}")]
        [Authorize(Roles = "Client,Admin")]
        public IActionResult GetWorkshopsById(int id)
        {
            var workshopBooking = _authDbContext.WorkshopBooking.Find(id);

            if (workshopBooking is null)
                return NotFound();

            if (!CanAccess(workshopBooking))
                return StatusCode(StatusCodes.Status403Forbidden);

            return Ok(workshopBooking);
        }

        [HttpGet("client-workshops")]
        [Authorize(Roles = "Client,Admin")]
        public IActionResult GetClientBookings()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var bookings = _authDbContext.WorkshopBooking
                .Where(b => b.UserId == userId)
                .ToList();
            return Ok(bookings);

        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public IActionResult AddWorkshopBookings(AddWorkshopBookingDTO WorkshopBookingentity) { 
            
            var WorkshopBooking = new WorkshopBooking() {
                
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                InstitutionName = WorkshopBookingentity.InstitutionName, 
                TargetAudience = WorkshopBookingentity.TargetAudience, 
                Topic = WorkshopBookingentity.Topic, 
                Status = "Scheduled",
                PhoneNumber = WorkshopBookingentity.PhoneNumber,
                EmailAddress = WorkshopBookingentity.EmailAddress, 
                Date_and_Time = WorkshopBookingentity.Date_and_Time, 
                OptionalNotes = WorkshopBookingentity.OptionalNotes }; 
            
            _authDbContext.WorkshopBooking.Add(WorkshopBooking); 
            _authDbContext.SaveChanges();
            return Ok(WorkshopBooking);
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "Client,Admin")]
        public IActionResult UpdateWorkshop(int id, UpdateWorkshopBookingDTO WorkshopBookingentity) {
            var workshopBooking = _authDbContext.WorkshopBooking.Find(id); 
            
            if (workshopBooking is null) return NotFound();
            
            if (!CanAccess(workshopBooking))
                return StatusCode(StatusCodes.Status403Forbidden); 
            workshopBooking.InstitutionName = WorkshopBookingentity.InstitutionName; 
            workshopBooking.TargetAudience = WorkshopBookingentity.TargetAudience; 
            workshopBooking.EmailAddress = WorkshopBookingentity.EmailAddress;
            workshopBooking.Topic = WorkshopBookingentity.Topic;
            workshopBooking.PhoneNumber = WorkshopBookingentity.PhoneNumber; 
            workshopBooking.Date_and_Time = WorkshopBookingentity.Date_and_Time;
            workshopBooking.OptionalNotes = WorkshopBookingentity.OptionalNotes; 
            
            if (User.IsInRole("Admin") && !string.IsNullOrWhiteSpace(WorkshopBookingentity.Status)) { 
                
                workshopBooking.Status = WorkshopBookingentity.Status; 
            } 
            _authDbContext.SaveChanges();
            return Ok(workshopBooking); 
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,Client")]
        [Route("{id:int}")]
        public IActionResult DeleteWorkshopBooking(int id)
        {
            var workshopBooking = _authDbContext.WorkshopBooking.Find(id);
            
            if (workshopBooking is null)
                
                return NotFound();

            if (!CanAccess(workshopBooking))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden);
            }

            _authDbContext.WorkshopBooking.Remove(workshopBooking);
            
            _authDbContext.SaveChanges();
            
            return Ok();
        }

        
        


    }
}