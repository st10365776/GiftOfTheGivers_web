using System.Security.Claims;
using GiftOfThe_Givers_web.Data;
using GiftOfTheGivers_web.Models;
using GiftOfTheGivers_web.Models.ViewModels;
using GiftOfTheGivers_web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers_web.Controllers;

public class EmployeeController : Controller
{
    private readonly ApplicationDbContext _db;
    public EmployeeController(ApplicationDbContext db) => _db = db;

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Dashboard));
        ViewBag.ReturnUrl = returnUrl;
        return View(new EmployeeLoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(EmployeeLoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        var employee = await _db.Employees.SingleOrDefaultAsync(e => e.Email.ToLower() == email && e.IsActive);
        if (employee == null || !PasswordService.Verify(model.Password, employee.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid email address or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, employee.EmployeeID.ToString()),
            new(ClaimTypes.Name, employee.FullName),
            new(ClaimTypes.Email, employee.Email),
            new(ClaimTypes.Role, employee.Role)
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        return LocalRedirect(returnUrl ?? Url.Action(nameof(Dashboard))!);
    }

    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> Dashboard()
    {
        var vm = new EmployeeDashboardViewModel
        {
            TotalDonations = await _db.Donations.Where(d => d.Status != "Cancelled").SumAsync(d => (decimal?)d.Amount) ?? 0,
            DonationCount = await _db.Donations.CountAsync(),
            VolunteerCount = await _db.Volunteers.CountAsync(),
            ActiveOperations = await _db.ReliefProjects.CountAsync(p => p.Status == "Active"),
            OperationsNeedingAttention = await _db.ReliefProjects.CountAsync(p => p.Status == "Attention"),
            RecentDonations = await _db.Donations.Include(d => d.User).OrderByDescending(d => d.DonationDate).Take(6).ToListAsync(),
            RecentOperations = await _db.ReliefProjects.OrderByDescending(p => p.StartDate).Take(5).ToListAsync()
        };
        return View(vm);
    }

    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> Donations(string? status = null)
    {
        var query = _db.Donations.Include(d => d.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(d => d.Status == status);
        ViewBag.Status = status;
        return View(await query.OrderByDescending(d => d.DonationDate).ToListAsync());
    }

    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> DonationDetails(int id)
    {
        var donation = await _db.Donations.Include(d => d.User).FirstOrDefaultAsync(d => d.DonationID == id);
        if (donation == null) return NotFound();
        return View(donation);
    }

    [Authorize(Roles = "Employee,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDonationStatus(int id, string status)
    {
        var donation = await _db.Donations.FindAsync(id);
        if (donation == null) return NotFound();
        donation.Status = status;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(DonationDetails), new { id });
    }

    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> Volunteers(string? status = null)
    {
        var query = _db.Volunteers.Include(v => v.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(v => v.Status == status);
        ViewBag.Status = status;
        return View(await query.OrderByDescending(v => v.AppliedDate).ToListAsync());
    }

    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> VolunteerDetails(int id)
    {
        var volunteer = await _db.Volunteers.Include(v => v.User).Include(v => v.VolunteerProjects).ThenInclude(vp => vp.ReliefProject).FirstOrDefaultAsync(v => v.VolunteerID == id);
        if (volunteer == null) return NotFound();
        return View(volunteer);
    }

    [Authorize(Roles = "Employee,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateVolunteerStatus(int id, string status)
    {
        var volunteer = await _db.Volunteers.FindAsync(id);
        if (volunteer == null) return NotFound();
        volunteer.Status = status;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(VolunteerDetails), new { id });
    }

    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> ReliefOperations()
        => View(await _db.ReliefProjects.OrderByDescending(p => p.StartDate).ToListAsync());

    [Authorize(Roles = "Employee,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveReliefOperation(ReliefProject model)
    {
        if (!ModelState.IsValid) return View(nameof(ReliefOperations), await _db.ReliefProjects.ToListAsync());
        if (model.ProjectID == 0) _db.ReliefProjects.Add(model); else _db.ReliefProjects.Update(model);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(ReliefOperations));
    }

    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> Reports()
    {
        ViewBag.Total = await _db.Donations.Where(d => d.Status != "Cancelled").SumAsync(d => (decimal?)d.Amount) ?? 0;
        ViewBag.Completed = await _db.Donations.CountAsync(d => d.Status == "Completed");
        ViewBag.PendingDonations = await _db.Donations.CountAsync(d => d.Status == "Pending");
        ViewBag.PendingVolunteers = await _db.Volunteers.CountAsync(v => v.Status == "Pending");
        ViewBag.ActiveProjects = await _db.ReliefProjects.CountAsync(p => p.Status == "Active");
        ViewBag.Projects = await _db.ReliefProjects.CountAsync();
        return View();
    }

    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> Settings()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var employee = await _db.Employees.FindAsync(id);
        return employee == null ? NotFound() : View(employee);
    }

    [Authorize(Roles = "Employee,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(Employee model, string? newPassword)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var employee = await _db.Employees.FindAsync(id);
        if (employee == null) return NotFound();
        employee.FullName = model.FullName.Trim();
        employee.PhoneNumber = model.PhoneNumber?.Trim();
        if (!string.IsNullOrWhiteSpace(newPassword)) employee.PasswordHash = PasswordService.Hash(newPassword);
        await _db.SaveChangesAsync();
        TempData["Message"] = "Your settings have been updated.";
        return RedirectToAction(nameof(Settings));
    }
}
