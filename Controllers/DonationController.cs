using Microsoft.AspNetCore.Mvc;
using GiftOfTheGivers_web.Models;

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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return RedirectToAction(nameof(Details), model);
        }

        [HttpGet]
        public IActionResult Details(DonationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Complete(DonationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Details", model);
            }

            return RedirectToAction(nameof(ThankYou), new { anonymous = model.IsAnonymous });
        }

        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }

        [HttpGet]
        public IActionResult History()
        {
            return View();
        }
    }
}