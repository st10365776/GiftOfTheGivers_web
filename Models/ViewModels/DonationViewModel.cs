// Carries donation selections between the form and review pages.
using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers_web.Models
{
    public class DonationViewModel
    {
        [Required]
        public string DonationType { get; set; } = "Once-off";

        [Required]
        public string Currency { get; set; } = "ZAR";

        [Required]
        [Range(1, 1000000)]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Card";

        public bool IsAnonymous { get; set; } = true;
    }
}