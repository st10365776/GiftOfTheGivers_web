using System.Security.Claims;
using GiftOfThe_Givers_web.Data;
using GiftOfTheGivers_web.Models;
using GiftOfTheGivers_web.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers_web.Controllers;

public class VolunteerController : Controller
{
    private readonly ApplicationDbContext _db;

    public VolunteerController(
        ApplicationDbContext db)
    {
        _db = db;
    }

    // =====================================================
    // VOLUNTEER INFORMATION PAGE
    // =====================================================

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // =====================================================
    // APPLY
    // =====================================================

    [Authorize(Roles = "User")]
    [HttpGet]
    public async Task<IActionResult> Apply()
    {
        var userId = GetLoggedInUserId();

        if (userId == null)
        {
            return RedirectToAction(
                "Login",
                "Account",
                new
                {
                    returnUrl =
                        Url.Action(
                            nameof(Apply),
                            "Volunteer")
                });
        }

        var user = await _db.Users
            .Include(u => u.Volunteer)
            .FirstOrDefaultAsync(
                u => u.UserID == userId.Value);

        if (user == null)
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(
                "Login",
                "Account");
        }

        // Prevent multiple applications.
        if (user.Volunteer != null)
        {
            TempData["VolunteerMessage"] =
                "You already have a volunteer application.";

            return RedirectToAction(
                "Profile",
                "Account");
        }

        var names =
            (user.FullName ?? string.Empty)
            .Trim()
            .Split(
                ' ',
                2,
                StringSplitOptions.RemoveEmptyEntries);

        return View(
            new VolunteerApplicationViewModel
            {
                FirstName =
                    names.Length > 0
                        ? names[0]
                        : string.Empty,

                LastName =
                    names.Length > 1
                        ? names[1]
                        : string.Empty,

                Email = user.Email,

                Phone =
                    user.PhoneNumber ?? string.Empty
            });
    }

    // =====================================================
    // SUBMIT APPLICATION
    // =====================================================

    [Authorize(Roles = "User")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(
        VolunteerApplicationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Apply", model);
        }

        var userId = GetLoggedInUserId();

        if (userId == null)
        {
            return RedirectToAction(
                "Login",
                "Account");
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(
                u => u.UserID == userId.Value);

        if (user == null)
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(
                "Login",
                "Account");
        }

        // Check again before creating the record.
        var alreadyApplied =
            await _db.Volunteers
                .AnyAsync(
                    v => v.UserID == userId.Value);

        if (alreadyApplied)
        {
            TempData["VolunteerMessage"] =
                "You already have a volunteer application.";

            return RedirectToAction(
                "Profile",
                "Account");
        }

        var volunteer = new Volunteer
        {
            UserID = userId.Value,

            Skills =
                string.IsNullOrWhiteSpace(model.About)
                    ? model.Area
                    : $"{model.Area} | {model.About.Trim()}",

            Availability =
                model.Availability.Trim(),

            Location =
                model.Location.Trim(),

            Status = "Pending",

            AppliedDate = DateTime.UtcNow
        };

        _db.Volunteers.Add(volunteer);

        await _db.SaveChangesAsync();

        model.FirstName = user.FullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? string.Empty;
        model.LastName = string.Join(
            " ",
            user.FullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1));
        model.Email = user.Email;
        model.Phone = user.PhoneNumber ?? string.Empty;

        return View(
            "Confirm",
            model);
    }

    // =====================================================
    // DETAILS
    // =====================================================

    [HttpGet]
    public IActionResult Details()
    {
        return View();
    }

    // =====================================================
    // HELPER
    // =====================================================

    private int? GetLoggedInUserId()
    {
        var claim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (int.TryParse(claim, out var userId))
        {
            return userId;
        }

        return null;
    }
}