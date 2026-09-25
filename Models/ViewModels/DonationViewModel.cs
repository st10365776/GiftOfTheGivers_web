using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers_web.Models.ViewModels;

public class DonationViewModel
{
    [Required]
    [Range(
        1,
        100000000,
        ErrorMessage = "Please enter a valid donation amount.")]
    public decimal Amount { get; set; }

    [Required]
    public string Currency { get; set; } = "ZAR";

    [Required]
    public string DonationType { get; set; } = "Once-off";

    [Required]
    public string PaymentMethod { get; set; } = "Card";

    public bool IsAnonymous { get; set; }
}