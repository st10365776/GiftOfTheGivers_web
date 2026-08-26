using GiftOfTheGivers_web.Models;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers_web.Controllers
{
    public class DonationController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(DonationViewModel model)
        {
            if (model.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Please select a donation amount.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return View("Details", model);
        }

        public IActionResult OnceOff()
        {
            return RedirectToAction("Index");
        }

        public IActionResult Monthly()
        {
            return RedirectToAction("Index");
        }

        public IActionResult Details()
        {
            return View();
        }

        public IActionResult ThankYou()
        {
            return View();
        }
    }
}