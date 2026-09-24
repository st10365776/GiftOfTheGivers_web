using System.Security.Claims;
using GiftOfThe_Givers_web.Data;
using GiftOfTheGivers_web.Models;
using GiftOfTheGivers_web.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GiftOfTheGivers_web.Controllers;

public class DonationController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DonationController> _logger;

    public DonationController(
        ApplicationDbContext db,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<DonationController> logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    // =====================================================
    // DONATION PAGE
    // =====================================================

    [HttpGet]
    public IActionResult Index()
    {
        var loggedIn =
            User.Identity?.IsAuthenticated == true &&
            User.IsInRole("User");

        return View(
            new DonationViewModel
            {
                // Guests are automatically anonymous.
                IsAnonymous = !loggedIn
            });
    }

    // =====================================================
    // SUBMIT DONATION DETAILS
    // =====================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(
        DonationViewModel model)
    {
        var loggedIn =
            User.Identity?.IsAuthenticated == true &&
            User.IsInRole("User");

        // Guests cannot reveal an identity.
        if (!loggedIn)
        {
            model.IsAnonymous = true;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(
            nameof(Details),
            new
            {
                Amount = model.Amount,
                Currency = model.Currency,
                DonationType = model.DonationType,
                PaymentMethod = model.PaymentMethod,
                IsAnonymous = model.IsAnonymous
            });
    }

    // =====================================================
    // DONATION REVIEW
    // =====================================================

    [HttpGet]
    public IActionResult Details(
        DonationViewModel model)
    {
        if (model.Amount <= 0)
        {
            return RedirectToAction(nameof(Index));
        }

        var loggedIn =
            User.Identity?.IsAuthenticated == true &&
            User.IsInRole("User");

        if (!loggedIn)
        {
            model.IsAnonymous = true;
        }

        return View(model);
    }

    // =====================================================
    // COMPLETE DONATION
    // =====================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(
        DonationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Details", model);
        }

        var loggedIn =
            User.Identity?.IsAuthenticated == true &&
            User.IsInRole("User");

        int? userId = null;

        if (loggedIn)
        {
            var claim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (int.TryParse(claim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            if (userId == null ||
                !await _db.Users.AnyAsync(u => u.UserID == userId.Value))
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction(
                    "Login",
                    "Account",
                    new
                    {
                        returnUrl = Url.Action(nameof(Index))
                    });
            }
        }

        // If not logged in, UserID stays null.
        // Therefore the donation is anonymous.
        var donation = new Donation
        {
            Amount = model.Amount,

            Currency = model.Currency,

            DonationType = model.DonationType,

            PaymentMethod = model.PaymentMethod,

            UserID = userId,

            IsAnonymous =
                !loggedIn || model.IsAnonymous,

            Status = "Pending",

            DonationDate = DateTime.UtcNow,

            Description =
                !loggedIn
                    ? "Anonymous website donation"
                    : "Website donation"
        };

        _db.Donations.Add(donation);

        await _db.SaveChangesAsync();

        if (await GenerateCertificateAsync(donation))
        {
            await _db.SaveChangesAsync();
            TempData["CertificateNumber"] = donation.CertificateNumber;
        }
        else
        {
            TempData["CertificateError"] =
                "Your donation was recorded, but your certificate could not be generated yet.";
        }

        return RedirectToAction(
            nameof(ThankYou),
            new
            {
                anonymous = donation.IsAnonymous,
                donationId = donation.DonationID
            });
    }

    // =====================================================
    // THANK YOU
    // =====================================================

    [HttpGet]
    public IActionResult ThankYou(
        bool anonymous = false,
        int? donationId = null)
    {
        ViewBag.IsAnonymous = anonymous;
        ViewBag.DonationId = donationId;

        return View();
    }

    // =====================================================
    // USER DONATION HISTORY
    // =====================================================

    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<IActionResult> History()
    {
        var claim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(claim, out var userId))
        {
            return RedirectToAction(
                "Login",
                "Account");
        }

        var userExists = await _db.Users.AnyAsync(u => u.UserID == userId);
        if (!userExists)
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }

        var donations = await _db.Donations
            .Where(d => d.UserID == userId)
            .OrderByDescending(
                d => d.DonationDate)
            .ToListAsync();

        return View(donations);
    }

    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<IActionResult> Certificate(int id)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claim, out var userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var donation = await _db.Donations
            .FirstOrDefaultAsync(d =>
                d.DonationID == id &&
                d.UserID == userId);

        if (donation?.CertificateNumber == null ||
            donation.CertificateIssuedAtUtc == null)
        {
            return NotFound();
        }

        return View(donation);
    }

    private async Task<bool> GenerateCertificateAsync(Donation donation)
    {
        var endpoint = _configuration["CertificateFunction:Endpoint"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            _logger.LogError(
                "Certificate generation is not configured for donation {DonationId}.",
                donation.DonationID);

            return false;
        }

        var donorName = donation.IsAnonymous
            ? "Anonymous Donor"
            : User.Identity?.Name ?? "Gift of the Givers Donor";

        var requestUrl = QueryHelpers.AddQueryString(
            endpoint,
            new Dictionary<string, string?>
            {
                ["donorName"] = donorName,
                ["donationAmount"] = donation.Amount.ToString(
                    System.Globalization.CultureInfo.InvariantCulture)
            });

        try
        {
            var client = _httpClientFactory.CreateClient("CertificateFunction");
            using var response = await client.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Certificate Function returned {StatusCode} for donation {DonationId}.",
                    response.StatusCode,
                    donation.DonationID);

                return false;
            }

            await using var stream =
                await response.Content.ReadAsStreamAsync();

            var certificate = await JsonSerializer.DeserializeAsync<
                CertificateGenerationResponse>(
                stream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (certificate is not
                {
                    Success: true,
                    CertificateNumber.Length: > 0
                })
            {
                _logger.LogError(
                    "Certificate Function returned an invalid result for donation {DonationId}.",
                    donation.DonationID);

                return false;
            }

            donation.CertificateNumber = certificate.CertificateNumber;
            donation.CertificateIssuedAtUtc = certificate.IssuedAtUtc;

            return true;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "Certificate Function request failed for donation {DonationId}.",
                donation.DonationID);
        }
        catch (JsonException exception)
        {
            _logger.LogError(
                exception,
                "Certificate Function returned malformed JSON for donation {DonationId}.",
                donation.DonationID);
        }

        return false;
    }
}