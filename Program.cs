using GiftOfThe_Givers_web.Data;
using GiftOfTheGivers_web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// DATABASE
// ---------------------------------------------------------

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// ---------------------------------------------------------
// PASSWORD HASHING
// ---------------------------------------------------------

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddHttpClient("CertificateFunction", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});

// ---------------------------------------------------------
// AUTHENTICATION
// ---------------------------------------------------------

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/";

        // Keep users logged in for 30 days.
        options.ExpireTimeSpan = TimeSpan.FromDays(30);

        // Extend the cookie while the user is active.
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ---------------------------------------------------------
// MVC
// ---------------------------------------------------------

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ---------------------------------------------------------
// HTTP PIPELINE
// ---------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// ---------------------------------------------------------
// DEFAULT ROUTE
// ---------------------------------------------------------

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();