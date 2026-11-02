using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartRoom.Data;
using SmartRoom.Models.Enums;

namespace SmartRoom.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(returnUrl ?? "/");
        }

        ViewBag.ReturnUrl = returnUrl;
        var users = await _context.Users.OrderBy(u => u.Role).ToListAsync();
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string? returnUrl)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => 
            u.Username.ToLower() == username.Trim().ToLower() || 
            u.Email.ToLower() == username.Trim().ToLower());

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Pengguna tidak ditemukan. Silakan pilih salah satu persona simulasi di bawah.");
            var users = await _context.Users.OrderBy(u => u.Role).ToListAsync();
            return View(users);
        }

        await SignInUserAsync(user);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> SwitchRole(string role, string? returnUrl)
    {
        if (Enum.TryParse<UserRole>(role, true, out var targetRole))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Role == targetRole);
            if (user != null)
            {
                await SignInUserAsync(user);
                TempData["SuccessMessage"] = $"Beralih peran aktif sebagai {user.Role}: {user.FullName} ({user.Department})";
            }
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "Anda telah keluar dari sesi.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task SignInUserAsync(Models.User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("Department", user.Department),
            new("AvatarInitials", user.AvatarInitials),
            new("Username", user.Username)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}
