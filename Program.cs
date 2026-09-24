using GiftOfThe_Givers_web.Data;
using GiftOfTheGivers_web.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// =====================================================
// DATABASE CONNECTION
// =====================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));


// =====================================================
// COOKIE AUTHENTICATION
// =====================================================

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme
)
.AddCookie(options =>
{
    // Employee/Admin protected pages will redirect here
    // when the employee is not logged in.
    options.LoginPath = "/Employee/Login";

    options.AccessDeniedPath = "/Employee/Login";

    // Keep users logged in for 8 hours.
    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    // Extend the cookie while the user is active.
    options.SlidingExpiration = true;
});


// =====================================================
// AUTHORIZATION
// =====================================================

builder.Services.AddAuthorization();


// =====================================================
// MVC
// =====================================================

builder.Services.AddControllersWithViews();


var app = builder.Build();


// =====================================================
// HTTP REQUEST PIPELINE
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


// Authentication MUST come before Authorization
app.UseAuthentication();

app.UseAuthorization();


// =====================================================
// DEFAULT MVC ROUTE
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);


app.Run();