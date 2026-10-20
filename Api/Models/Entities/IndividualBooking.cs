using System.ComponentModel.DataAnnotations;

namespace Api.Models.Entities
{
    public class IndividualBooking
    {
        [Key] 
        public int IndividualBookingID { get; set; }
        public string UserId { get; set; }

        [Display(Name = "First Name")] 
        public string FirstName { get; set; }

        [Display(Name = "Surname")]
        public string Surname { get; set; }

        public string Category { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email Address")] 
        public string EmailAddress { get; set; }

        [Display(Name = "Date and Time")] 
        public DateTime Date_and_Time { get; set; }

        [Display(Name = "Optional Notes")] 
        public string? OptionalNotes { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Session Notes")]
        public string? SessionNotes { get; set; }
    }
}
