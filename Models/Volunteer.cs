// Represents a volunteer who applies to support the organisation.
namespace GiftOfTheGivers_web.Models
{
    public class Volunteer
    {
        public int VolunteerID { get; set; }

        public string? Skills { get; set; }

        public string? Availability { get; set; }

        public string? Location { get; set; }

        public int UserID { get; set; }

        // RELATIONSHIPS
        public User? User { get; set; }

        public ICollection<VolunteerProject> VolunteerProjects { get; set; }
            = new List<VolunteerProject>();
    }
}