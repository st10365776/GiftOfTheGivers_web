using GiftOfThe_Givers_web.Data;
using GiftOfTheGivers_web.Models;
using GiftOfTheGivers_web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers_web.Controllers;

public class VolunteerController : Controller
{
    private readonly ApplicationDbContext _db;
    public VolunteerController(ApplicationDbContext db) => _db = db;
    public IActionResult Index() => View();
    public IActionResult Apply() => View(new VolunteerApplicationViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(VolunteerApplicationViewModel model)
    {
        if (!ModelState.IsValid) return View("Apply", model);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null)
        {
            user = new User { FullName = $"{model.FirstName} {model.LastName}", Email = model.Email, Password = string.Empty, Role = "Volunteer", PhoneNumber = model.Phone };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        var volunteer = new Volunteer { UserID = user.UserID, Skills = model.Area + (string.IsNullOrWhiteSpace(model.About) ? "" : " | " + model.About), Availability = model.Availability, Location = model.Location, Status = "Pending", AppliedDate = DateTime.UtcNow };
        _db.Volunteers.Add(volunteer);
        await _db.SaveChangesAsync();
        return View("Confirm", model);
    }
    public IActionResult Details() => View();
}
