namespace GiftOfTheGivers.Helpers;

public static class CertificateNumberFormatter
{
    public static string Format(string certificateNumber)
    {
        if (string.IsNullOrWhiteSpace(certificateNumber))
        {
            throw new ArgumentException(
                "A certificate number is required.",
                nameof(certificateNumber));
        }

        return certificateNumber.Trim().ToUpperInvariant();
    }
}
