namespace GiftOfTheGivers_web.Models.ViewModels;

public class EmployeeDashboardViewModel
{
    public decimal TotalDonations { get; set; }
    public int DonationCount { get; set; }
    public int VolunteerCount { get; set; }
    public int ActiveOperations { get; set; }
    public int OperationsNeedingAttention { get; set; }
    public List<Donation> RecentDonations { get; set; } = new();
    public List<ReliefProject> RecentOperations { get; set; } = new();
}
