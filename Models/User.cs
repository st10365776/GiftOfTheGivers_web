// Represents a person with an account on the website.
namespace GiftOfTheGivers_web.Models
{
    public class User
    {
        public int UserID { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }

        public string? PhoneNumber { get; set; }

        // RELATIONSHIPS
        public Volunteer? Volunteer { get; set; }

        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }
}
