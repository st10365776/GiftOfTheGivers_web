namespace GiftOfTheGivers_web.Models;

public class Donation
{
    public int DonationID { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZAR";
    public DateTime DonationDate { get; set; } = DateTime.UtcNow;
    public string DonationType { get; set; } = "Once-off";
    public string PaymentMethod { get; set; } = "Card";
    public string Status { get; set; } = "Completed";
    public bool IsAnonymous { get; set; } = true;
    public string? Description { get; set; }
    public string? CertificateNumber { get; set; }
    public DateTime? CertificateIssuedAtUtc { get; set; }
    public int? UserID { get; set; }
    public User? User { get; set; }
}
