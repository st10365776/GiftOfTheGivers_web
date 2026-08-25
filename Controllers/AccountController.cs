using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers_web.Controllers;

public class AccountController : Controller
{
    public IActionResult Login() => View();

    public IActionResult Register() => View();

    public IActionResult Profile() => View();
}
