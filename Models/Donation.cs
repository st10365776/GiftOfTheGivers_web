// Represents a donation in the application domain.
namespace GiftOfTheGivers_web.Models
{
    public class Donation
    {
        public int DonationID { get; set; }

        public decimal Amount { get; set; }

        public DateTime DonationDate { get; set; }

        public string DonationType { get; set; }

        public string? Description { get; set; }

        public int UserID { get; set; }

        // RELATIONSHIP
        public User? User { get; set; }
    }
}