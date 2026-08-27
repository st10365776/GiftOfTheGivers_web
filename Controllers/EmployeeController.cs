using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers_web.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Donations()
        {
            return View();
        }

        public IActionResult DonationDetails()
        {
            return View();
        }

        public IActionResult Volunteers()
        {
            return View();
        }

        public IActionResult VolunteerDetails()
        {
            return View();
        }
    }
}