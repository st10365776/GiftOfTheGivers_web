using GiftOfTheGivers_web.Models;
using Microsoft.EntityFrameworkCore;

// Defines the database context used by the application.
namespace GiftOfThe_Givers_web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Database tables
        public DbSet<User> Users { get; set; }
        public DbSet<Volunteer> Volunteers { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<ReliefProject> ReliefProjects { get; set; }
        public DbSet<VolunteerProject> VolunteerProjects { get; set; }
    }
}