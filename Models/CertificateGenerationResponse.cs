namespace GiftOfTheGivers_web.Models;

public sealed class CertificateGenerationResponse
{
    public bool Success { get; init; }
    public string CertificateNumber { get; init; } = string.Empty;
    public DateTime IssuedAtUtc { get; init; }
}
