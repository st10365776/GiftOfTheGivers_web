// Holds the details shown when a request ends in an error.
namespace GiftOfTheGivers_web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}