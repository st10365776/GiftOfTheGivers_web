using GiftOfTheGivers.Helpers;
using Xunit;

namespace GiftOfTheGivers_web.Tests;

public class CertificateNumberFormatterTests
{
    [Fact]
    public void FormatTrimsWhitespaceAndNormalizesToUppercase()
    {
        var formatted = CertificateNumberFormatter.Format("  gotg-2026-ab12  ");

        Assert.Equal("GOTG-2026-AB12", formatted);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void FormatRejectsMissingCertificateNumbers(string? certificateNumber)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => CertificateNumberFormatter.Format(certificateNumber!));

        Assert.Equal("certificateNumber", exception.ParamName);
    }
}
