using GiftOfThe_Givers_web.Data;
using GiftOfTheGivers_web.Models;
using GiftOfTheGivers_web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers_web.Controllers;

public class DonationController : Controller
{
    private readonly ApplicationDbContext _db;
    public DonationController(ApplicationDbContext db) => _db = db;

    [HttpGet] public IActionResult Index() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(DonationViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        return RedirectToAction(nameof(Details), model);
    }

    [HttpGet]
    public IActionResult Details(DonationViewModel model)
        => !ModelState.IsValid ? RedirectToAction(nameof(Index)) : View(model);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(DonationViewModel model)
    {
        if (!ModelState.IsValid) return View("Details", model);
        var donation = new Donation
        {
            Amount = model.Amount,
            Currency = model.Currency,
            DonationType = model.DonationType,
            PaymentMethod = model.PaymentMethod,
            IsAnonymous = model.IsAnonymous,
            Status = "Pending",
            DonationDate = DateTime.UtcNow,
            Description = "Donation submitted through the website"
        };
        _db.Donations.Add(donation);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(ThankYou), new { anonymous = model.IsAnonymous });
    }

    [HttpGet] public IActionResult ThankYou() => View();

    [HttpGet] public async Task<IActionResult> History()
        => View(await _db.Donations.OrderByDescending(d => d.DonationDate).Take(50).ToListAsync());
}
