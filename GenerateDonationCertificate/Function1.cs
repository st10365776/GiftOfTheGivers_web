using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GenerateDonationCertificate;

public class GenerateDonationCertificate(
    ILogger<GenerateDonationCertificate> logger)
{
    [Function("GenerateDonationCertificate")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
        HttpRequest request)
    {
        var donorName = request.Query["donorName"].ToString().Trim();
        var donationAmount = request.Query["donationAmount"].ToString();

        if (string.IsNullOrWhiteSpace(donorName))
        {
            return new BadRequestObjectResult(
                "A donorName query parameter is required.");
        }

        if (!decimal.TryParse(
                donationAmount,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var amount) ||
            amount <= 0)
        {
            return new BadRequestObjectResult(
                "donationAmount must be a positive decimal value.");
        }

        var certificateNumber =
            $"CERT-{DateTime.UtcNow:yyyy}-{Guid.NewGuid():N}"
                .ToUpperInvariant();

        logger.LogInformation(
            "Generated certificate {CertificateNumber} for {DonorName}.",
            certificateNumber,
            donorName);

        return new OkObjectResult(new
        {
            success = true,
            donorName,
            donationAmount = amount,
            certificateNumber,
            issuedAtUtc = DateTime.UtcNow,
            message = "Donation certificate generated successfully."
        });
    }
}
