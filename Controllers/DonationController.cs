using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers_web.Controllers;

public class DonationController : Controller
{
    public IActionResult Index() => View();

    public IActionResult OnceOff() => View();

    public IActionResult Monthly() => View();

    public IActionResult Confirm() => View();

    public IActionResult Details() => View();
}
