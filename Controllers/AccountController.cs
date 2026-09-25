using System.Security.Claims;
using GiftOfThe_Givers_web.Data;
using GiftOfTheGivers_web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers_web.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AccountController(
        ApplicationDbContext context,
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // =========================================================
    // REGISTER - GET
    // =========================================================

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Profile));
        }

        return View();
    }


    // =========================================================
    // REGISTER - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        User user,
        string confirmPassword)
    {
        user.Email =
            user.Email?.Trim().ToLowerInvariant() ?? "";

        user.FullName =
            user.FullName?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(user.FullName))
        {
            ModelState.AddModelError(
                "FullName",
                "Full name is required.");
        }

        // Validate email
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            ModelState.AddModelError(
                "Email",
                "Email is required.");
        }

        // Validate password
        if (string.IsNullOrWhiteSpace(user.Password))
        {
            ModelState.AddModelError(
                "Password",
                "Password is required.");
        }

        // Confirm password
        if (user.Password != confirmPassword)
        {
            ModelState.AddModelError(
                "Password",
                "Passwords do not match.");
        }

        // Check whether email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Email == user.Email);

        if (existingUser != null)
        {
            ModelState.AddModelError(
                "Email",
                "An account with this email already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(user);
        }

        // New accounts are normal users
        user.Role = "User";

        // Securely hash password
        user.Password = _passwordHasher.HashPassword(
            user,
            user.Password);

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        TempData["RegisterMessage"] =
            "Your account has been created successfully. Please log in.";

        return RedirectToAction(nameof(Login));
    }


    // =========================================================
    // LOGIN - GET
    // =========================================================

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Already logged in
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Employee") ||
                User.IsInRole("Admin"))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Employee");
            }

            if (User.IsInRole("User"))
            {
                return RedirectToAction(nameof(Profile));
            }
        }

        ViewBag.ReturnUrl = returnUrl;

        return View();
    }


    // =========================================================
    // LOGIN - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string email,
        string password,
        bool rememberMe = false,
        string? returnUrl = null)
    {
        // Clean email
        email =
            email?.Trim().ToLowerInvariant() ?? "";

        // Validate email
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(
                "Email",
                "Please enter your email address.");

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        // Validate password
        if (string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(
                "Password",
                "Please enter your password.");

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        // -----------------------------------------------------
        // FIND USER
        // -----------------------------------------------------

        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Email == email);

        if (user == null)
        {
            ModelState.AddModelError(
                "",
                "Invalid email or password.");

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        // -----------------------------------------------------
        // VERIFY PASSWORD
        // -----------------------------------------------------

        bool passwordValid = false;
        bool needsHashUpgrade = false;

        try
        {
            var passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.Password,
                    password);

            if (passwordResult ==
                PasswordVerificationResult.Success)
            {
                passwordValid = true;
            }
            else if (passwordResult ==
                     PasswordVerificationResult.SuccessRehashNeeded)
            {
                passwordValid = true;
                needsHashUpgrade = true;
            }
        }
        catch
        {
            // This can happen if an existing account
            // has an older/plain-text password format.
        }

        // -----------------------------------------------------
        // SUPPORT OLD PLAIN-TEXT PASSWORDS
        // -----------------------------------------------------
        //
        // This is useful for existing accounts created before
        // password hashing was added.
        //
        // Once the user successfully logs in, the password
        // will automatically be converted to a secure hash.
        // -----------------------------------------------------

        if (!passwordValid &&
            user.Password == password)
        {
            passwordValid = true;
            needsHashUpgrade = true;
        }

        // -----------------------------------------------------
        // PASSWORD INVALID
        // -----------------------------------------------------

        if (!passwordValid)
        {
            ModelState.AddModelError(
                "",
                "Invalid email or password.");

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        // -----------------------------------------------------
        // UPGRADE OLD PASSWORD
        // -----------------------------------------------------

        if (needsHashUpgrade)
        {
            user.Password =
                _passwordHasher.HashPassword(
                    user,
                    password);

            await _context.SaveChangesAsync();
        }

        // -----------------------------------------------------
        // CREATE CLAIMS
        // -----------------------------------------------------

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserID.ToString()),

            new Claim(
                ClaimTypes.Name,
                string.IsNullOrWhiteSpace(user.FullName)
                    ? user.Email
                    : user.FullName),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        // -----------------------------------------------------
        // CREATE AUTHENTICATION COOKIE
        // -----------------------------------------------------

        var authProperties =
            new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                AllowRefresh = true
            };

        if (rememberMe)
        {
            authProperties.ExpiresUtc =
                DateTimeOffset.UtcNow.AddDays(30);
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties);

        // -----------------------------------------------------
        // RETURN TO ORIGINAL PAGE
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        // -----------------------------------------------------
        // EMPLOYEE / ADMIN
        // -----------------------------------------------------

        return RedirectToAction(nameof(Profile));
    }


    // =========================================================
    // PROFILE - GET
    // =========================================================

    [HttpGet]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Profile()
    {
        // User must be logged in
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction(
                nameof(Login),
                new
                {
                    returnUrl =
                        Url.Action(nameof(Profile))
                });
        }

        // Get logged-in user's ID
        var userIdClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(
                userIdClaim,
                out var userId))
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        // Load user and related information
        var user = await _context.Users
            .Include(u => u.Donations)
            .Include(u => u.Volunteer)
            .FirstOrDefaultAsync(
                u => u.UserID == userId);

        if (user == null)
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        return View(user);
    }


    // =========================================================
    // PROFILE - POST / UPDATE
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Profile(User model)
    {
        // Must be logged in
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction(nameof(Login));
        }

        // Get logged-in user's ID
        var userIdClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(
                userIdClaim,
                out var userId))
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        // Load actual database user
        var user = await _context.Users
            .Include(u => u.Donations)
            .Include(u => u.Volunteer)
            .FirstOrDefaultAsync(
                u => u.UserID == userId);

        if (user == null)
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        // Clean email
        var email =
            model.Email?.Trim().ToLowerInvariant() ?? "";

        if (string.IsNullOrWhiteSpace(model.FullName))
        {
            ModelState.AddModelError(
                "FullName",
                "Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(
                "Email",
                "Email is required.");
        }

        // Check whether another user already uses email
        var emailExists = await _context.Users
            .AnyAsync(u =>
                u.UserID != userId &&
                u.Email == email);

        if (emailExists)
        {
            ModelState.AddModelError(
                "Email",
                "That email address is already in use.");
        }

        if (!ModelState.IsValid)
        {
            return View(user);
        }

        // Update profile
        user.FullName =
            model.FullName?.Trim() ??
            user.FullName;

        user.Email = email;

        user.PhoneNumber =
            model.PhoneNumber?.Trim();

        await _context.SaveChangesAsync();

        // -----------------------------------------------------
        // REFRESH LOGIN COOKIE
        // -----------------------------------------------------

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserID.ToString()),

            new Claim(
                ClaimTypes.Name,
                string.IsNullOrWhiteSpace(user.FullName)
                    ? user.Email
                    : user.FullName),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(ClaimTypes.Role, "User")
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal =
            new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc =
                    DateTimeOffset.UtcNow.AddDays(30)
            });

        TempData["ProfileMessage"] =
            "Your profile has been updated successfully.";

        return RedirectToAction(nameof(Profile));
    }


    // =========================================================
    // LOGOUT
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(
            "Index",
            "Home");
    }
}