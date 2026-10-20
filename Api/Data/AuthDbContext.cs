using Api.Controllers;
using Api.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public class AuthDbContext : IdentityDbContext<IdentityUser>
    {
        public AuthDbContext(
             DbContextOptions<AuthDbContext> options)
             : base(options)
        {
        }

        public DbSet<IndividualBooking> IndividualBooking { get; set; }

        public DbSet<WorkshopBooking> WorkshopBooking { get; set; }

        public DbSet<Notices> Notices { get; set; }
    }
}
