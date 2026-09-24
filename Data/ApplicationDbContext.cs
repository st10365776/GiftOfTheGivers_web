using GiftOfTheGivers_web.Models;
using Microsoft.EntityFrameworkCore;

namespace GiftOfThe_Givers_web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Volunteer> Volunteers => Set<Volunteer>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<ReliefProject> ReliefProjects => Set<ReliefProject>();
    public DbSet<VolunteerProject> VolunteerProjects => Set<VolunteerProject>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        /* =========================
           PRIMARY KEYS
           ========================= */

        modelBuilder.Entity<User>()
            .HasKey(u => u.UserID);

        modelBuilder.Entity<Employee>()
            .HasKey(e => e.EmployeeID);

        modelBuilder.Entity<Volunteer>()
            .HasKey(v => v.VolunteerID);

        modelBuilder.Entity<Donation>()
            .HasKey(d => d.DonationID);

        modelBuilder.Entity<ReliefProject>()
            .HasKey(p => p.ProjectID);

        modelBuilder.Entity<VolunteerProject>()
            .HasKey(vp => vp.VolunteerProjectID);


        /* =========================
           UNIQUE EMAILS
           ========================= */

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();


        /* =========================
           DONATIONS
           ========================= */

        modelBuilder.Entity<Donation>()
            .Property(d => d.Amount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Donation>()
            .HasOne(d => d.User)
            .WithMany(u => u.Donations)
            .HasForeignKey(d => d.UserID)
            .OnDelete(DeleteBehavior.SetNull);


        /* =========================
           VOLUNTEERS
           ========================= */

        modelBuilder.Entity<Volunteer>()
            .HasOne(v => v.User)
            .WithOne(u => u.Volunteer)
            .HasForeignKey<Volunteer>(v => v.UserID)
            .OnDelete(DeleteBehavior.Cascade);


        /* =========================
           VOLUNTEER PROJECTS
           ========================= */

        modelBuilder.Entity<VolunteerProject>()
            .HasOne(vp => vp.Volunteer)
            .WithMany(v => v.VolunteerProjects)
            .HasForeignKey(vp => vp.VolunteerID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VolunteerProject>()
            .HasOne(vp => vp.ReliefProject)
            .WithMany(p => p.VolunteerProjects)
            .HasForeignKey(vp => vp.ProjectID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}