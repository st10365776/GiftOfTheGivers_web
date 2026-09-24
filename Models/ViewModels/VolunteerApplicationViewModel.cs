using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers_web.Models.ViewModels;

public class VolunteerApplicationViewModel
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Area { get; set; } = string.Empty;

    [Required]
    public string Availability { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;

    [Required]
    public string About { get; set; } = string.Empty;
}