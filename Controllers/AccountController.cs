using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartRoom.Data;
using SmartRoom.Models;
using SmartRoom.Models.Enums;

namespace SmartRoom.Controllers;

public class LoginViewModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(returnUrl ?? "/");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(string.Empty, "Username dan password wajib diisi.");
            return View(model);
        }

        var normalizedUsername = model.Username.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => 
            u.Username.ToLower() == normalizedUsername || 
            u.Email.ToLower() == normalizedUsername);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Username/email atau kata sandi tidak sesuai.");
            return View(model);
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Username/email atau kata sandi tidak sesuai.");
            return View(model);
        }

        await SignInUserAsync(user, model.RememberMe);

        TempData["SuccessMessage"] = $"Selamat datang kembali, {user.FullName}!";

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Konfirmasi kata sandi tidak cocok.");
            return View(model);
        }

        var normalizedUsername = model.Username.Trim().ToLowerInvariant();
        var normalizedEmail = model.Email.Trim().ToLowerInvariant();

        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername))
        {
            ModelState.AddModelError("Username", "Username sudah digunakan oleh akun lain.");
            return View(model);
        }

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
        {
            ModelState.AddModelError("Email", "Alamat email sudah terdaftar.");
            return View(model);
        }

        // Initials from full name
        var parts = model.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = parts.Length >= 2 
            ? $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant() 
            : (model.FullName.Length >= 2 ? model.FullName[..2].ToUpperInvariant() : "SR");

        var newUser = new User
        {
            Username = normalizedUsername,
            FullName = model.FullName.Trim(),
            Email = normalizedEmail,
            Department = model.Department.Trim(),
            Role = UserRole.Student, // Default registration is Student
            AvatarInitials = initials,
            CreatedAt = DateTime.UtcNow
        };
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, model.Password);

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();

        await SignInUserAsync(newUser, false);
        TempData["SuccessMessage"] = "Pendaftaran akun berhasil! Anda masuk sebagai Mahasiswa.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "Sesi akun Anda telah berhasil ditutup.";
        return RedirectToAction("Login");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickSwitch(string targetRole, string? returnUrl)
    {
        var targetUsername = targetRole?.ToLowerInvariant() switch
        {
            "admin" => "admin",
            "staff" => "staff",
            "student" => "student",
            _ => "admin"
        };

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == targetUsername);
        if (user != null)
        {
            await SignInUserAsync(user, false);
            TempData["SuccessMessage"] = $"Beralih peran ke: {user.FullName} ({user.Role})";
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task SignInUserAsync(User user, bool isPersistent)
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
            IsPersistent = isPersistent,
            ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(12)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}
