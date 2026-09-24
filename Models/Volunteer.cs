namespace GiftOfTheGivers_web.Models;

public class Volunteer
{
    public int VolunteerID { get; set; }
    public string? Skills { get; set; }
    public string? Availability { get; set; }
    public string? Location { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;
    public int UserID { get; set; }
    public User? User { get; set; }
    public ICollection<VolunteerProject> VolunteerProjects { get; set; } = new List<VolunteerProject>();
}
