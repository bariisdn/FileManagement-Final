using System.Security.Claims;
using FileManagement.Data;
using FileManagement.Models;
using FileManagement.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileManagement.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Check if the username or email is already in use
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);

        if (existingUser != null)
        {
            ModelState.AddModelError("", "Kullanıcı adı veya e-posta zaten kullanılıyor.");
            return View(model);
        }

        // Create a new user instance
        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password), // Hash the password
            Role = "User",
            CreatedOn = DateTime.Now
        };

        // Add the user to the database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Redirect to login after successful registration
        return RedirectToAction("Login", "Account");
    }
    
    // Kayıt İşlemleri Bitti
    
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == model.Username);
        
        if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Add the user ID as the NameIdentifier
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user's ID
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Geçersiz kullanıcı adı veya şifre.");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }
}
