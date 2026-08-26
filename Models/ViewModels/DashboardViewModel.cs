using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers_web.Models
{
    public class DonationViewModel
    {
        [Required]
        public string DonationType { get; set; } = "Once-off";

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Card";
    }
}