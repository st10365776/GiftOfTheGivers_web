using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers_web.Models
{
    public class ReliefProject
    {
        [Key]
        public int ProjectID { get; set; }

        public string ProjectName { get; set; }

        public string? Description { get; set; }

        public string? Location { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string Status { get; set; }

        // RELATIONSHIP
        public ICollection<VolunteerProject> VolunteerProjects { get; set; }
            = new List<VolunteerProject>();
    }
}