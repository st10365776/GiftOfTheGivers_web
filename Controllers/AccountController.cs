using GiftOfThe_Givers_web.Data;
using GiftOfTheGivers_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers_web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // REGISTER
        // =====================================================

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            User user,
            string confirmPassword)
        {
            // Check that passwords match
            if (user.Password != confirmPassword)
            {
                ModelState.AddModelError(
                    "Password",
                    "Passwords do not match.");
            }


            // Check if email already exists
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == user.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists.");
            }


            // If validation failed
            if (!ModelState.IsValid)
            {
                return View(user);
            }


            // Add new user
            _context.Users.Add(user);

            // Save user to Azure SQL
            await _context.SaveChangesAsync();


            // Go to Login
            return RedirectToAction("Login");
        }



        // =====================================================
        // LOGIN
        // =====================================================

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password,
            bool rememberMe = false)
        {
            // Find the user using their email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);


            // Check if user exists
            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View();
            }


            // Check password
            if (user.Password != password)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View();
            }


            // Login successful
            return RedirectToAction("Index", "Home");
        }
    }
}