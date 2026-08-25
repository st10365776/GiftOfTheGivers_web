using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers_web.Controllers;

public class EmployeeController : Controller
{
    public IActionResult Dashboard() => View();

    public IActionResult Donations() => View();

    public IActionResult DonationDetails() => View();

    public IActionResult Volunteers() => View();

    public IActionResult VolunteerDetails() => View();
}
