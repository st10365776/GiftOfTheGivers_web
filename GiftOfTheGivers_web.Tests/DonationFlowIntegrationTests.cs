using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using GiftOfThe_Givers_web.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace GiftOfTheGivers_web.Tests;

public class DonationFlowIntegrationTests
{
    [Fact]
    public async Task GuestDonationFormCallsCertificateFunctionAndPersistsDonation()
    {
        await using var factory = new DonationWebApplicationFactory();
        using var client = factory.CreateClient();

        var donationForm = await client.GetAsync("/Donation/Index");
        donationForm.EnsureSuccessStatusCode();
        var donationToken = await ExtractAntiForgeryToken(donationForm);

        var detailsResponse = await client.PostAsync(
            "/Donation/Index",
            CreateFormContent(
                donationToken,
                ("Amount", "250"),
                ("Currency", "ZAR"),
                ("DonationType", "Monthly"),
                ("PaymentMethod", "EFT"),
                ("IsAnonymous", "true")));

        detailsResponse.EnsureSuccessStatusCode();
        Assert.Contains("Donation Details", await detailsResponse.Content.ReadAsStringAsync());
        var completionToken = await ExtractAntiForgeryToken(detailsResponse);

        var thankYouResponse = await client.PostAsync(
            "/Donation/Complete",
            CreateFormContent(
                completionToken,
                ("Amount", "250"),
                ("Currency", "ZAR"),
                ("DonationType", "Monthly"),
                ("PaymentMethod", "EFT"),
                ("IsAnonymous", "true")));

        thankYouResponse.EnsureSuccessStatusCode();
        Assert.Contains("Thank You", await thankYouResponse.Content.ReadAsStringAsync());

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var donation = await db.Donations.SingleAsync();

        Assert.Equal(250m, donation.Amount);
        Assert.Equal("ZAR", donation.Currency);
        Assert.Equal("Monthly", donation.DonationType);
        Assert.Equal("EFT", donation.PaymentMethod);
        Assert.True(donation.IsAnonymous);
        Assert.Equal("Pending", donation.Status);
        Assert.Null(donation.UserID);
        Assert.Equal("GOTG-2026-INT001", donation.CertificateNumber);
        Assert.Equal(
            new DateTime(2026, 10, 29, 13, 0, 0, DateTimeKind.Utc),
            donation.CertificateIssuedAtUtc);

        Assert.Equal(1, factory.FunctionHandler.RequestCount);
        var functionQuery = QueryHelpers.ParseQuery(
            new Uri(factory.FunctionHandler.LastRequestUri).Query);
        Assert.Equal("Anonymous Donor", functionQuery["donorName"]);
        Assert.Equal("250", functionQuery["donationAmount"]);
    }

    private static async Task<string> ExtractAntiForgeryToken(HttpResponseMessage response)
    {
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.CultureInvariant);

        Assert.True(match.Success, "The form did not contain an anti-forgery token.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static FormUrlEncodedContent CreateFormContent(
        string antiForgeryToken,
        params (string Name, string Value)[] fields)
    {
        return new FormUrlEncodedContent(
            fields
                .Append((Name: "__RequestVerificationToken", Value: antiForgeryToken))
                .Select(field => new KeyValuePair<string, string>(field.Name, field.Value)));
    }

    private sealed class DonationWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"donation-tests-{Guid.NewGuid():N}";

        public CertificateFunctionHandler FunctionHandler { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["CertificateFunction:Endpoint"] = "https://certificate.test/generate"
                    });
            });

            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ApplicationDbContext>();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));

                services.AddHttpClient("CertificateFunction")
                    .ConfigurePrimaryHttpMessageHandler(
                        () => FunctionHandler);
            });
        }
    }

    private sealed class CertificateFunctionHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }
        public string LastRequestUri { get; private set; } = string.Empty;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            LastRequestUri = request.RequestUri?.ToString() ?? string.Empty;

            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                          "success": true,
                          "certificateNumber": "gotg-2026-int001",
                          "issuedAtUtc": "2026-10-29T13:00:00Z"
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                });
        }
    }
}
