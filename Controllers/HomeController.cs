using GiftOfThe_Givers_web.Data;
using GiftOfTheGivers_web.Models;
using GiftOfTheGivers_web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers_web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View(new ContactMessageViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactMessageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _db.ContactSubmissions.Add(new ContactSubmission
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = model.Email.Trim().ToLowerInvariant(),
            Phone = model.Phone.Trim(),
            Subject = model.Subject.Trim(),
            Message = model.Message.Trim(),
            SubmittedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        TempData["ContactMessage"] =
            "Thank you. Your message has been sent to our admin team.";

        return Redirect($"{Url.Action(nameof(Contact))}#contact-form");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}