using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;
using System.Security.Cryptography;
using System.Text;

namespace Web.Controllers;

public class AuthenticationController : Controller
{
    private readonly ApplicationDbContext _context;

    public AuthenticationController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: / (Login page)
    public IActionResult Login()
    {
        // If user is already logged in, redirect appropriately
        if (IsUserLoggedIn())
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == "admin@gmail.com")
            {
                return RedirectToAction("Index", "Admin");
            }
            return RedirectToAction("Home", "Home");
        }

        return View();
    }

    // POST: /login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Login", model);
        }

        // Hardcoded admin check
        if (model.Email == "admin@gmail.com" && model.Password == "admin123")
        {
            // Set admin session
            HttpContext.Session.SetString("UserId", "0");
            HttpContext.Session.SetString("UserName", "Admin");
            HttpContext.Session.SetString("UserEmail", "admin@gmail.com");
            
            return RedirectToAction("Index", "Admin");
        }

        // Find user by email for regular users
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        
        if (user == null)
        {
            ModelState.AddModelError("", "Invalid email or password");
            return View("Login", model);
        }

        // Verify password (simple hash comparison for demo)
        var hashedPassword = HashPassword(model.Password);
        if (user.PasswordHash != hashedPassword)
        {
            ModelState.AddModelError("", "Invalid email or password");
            return View("Login", model);
        }

        // Set session
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserEmail", user.Email);

        return RedirectToAction("Home", "Home");
    }

    // GET: /register
    public IActionResult Register()
    {
        return View();
    }

    // POST: /register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Check if email already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "Email already exists");
            return View(model);
        }

        // Create new user
        var user = new Users
        {
            Name = model.Name,
            Email = model.Email,
            PasswordHash = HashPassword(model.Password),
            TotalPoints = 0
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Auto-login after registration
        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserEmail", user.Email);

        return RedirectToAction("Home", "Home");
    }

    // POST: /logout
    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Redirect("/");
    }

    // Helper method to check if user is logged in
    private bool IsUserLoggedIn()
    {
        return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
    }

    // Helper method to hash password (hexdigest format to match Python script)
    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(hashedBytes).ToLower();
        }
    }
}

// View Models
public class LoginViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
