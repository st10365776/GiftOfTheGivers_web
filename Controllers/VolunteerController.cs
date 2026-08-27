// Handles the volunteer application and confirmation pages.
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers_web.Controllers;

public class VolunteerController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Apply() => View();

    public IActionResult Confirm() => View();

    public IActionResult Details() => View();
}
