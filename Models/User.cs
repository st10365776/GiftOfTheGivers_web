namespace GiftOfTheGivers_web.Models;

public class User
{
    public int UserID { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public string? PhoneNumber { get; set; }

    // Navigation properties
    public ICollection<Donation> Donations { get; set; } = new List<Donation>();

    public Volunteer? Volunteer { get; set; }
}