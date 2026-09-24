namespace GiftOfTheGivers_web.Models
{
    public class VolunteerProject
    {
        public int VolunteerProjectID { get; set; }

        public DateTime AssignedDate { get; set; }

        public string? Role { get; set; }

        public int VolunteerID { get; set; }

        public int ProjectID { get; set; }

        // RELATIONSHIPS
        public Volunteer? Volunteer { get; set; }

        public ReliefProject? ReliefProject { get; set; }
    }
}