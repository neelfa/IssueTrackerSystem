using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Controllers
{
    public class AccountController : BaseController
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context, IAuthService authService) : base(authService)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            return View("~/Views/Accounts/Register.cshtml");
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password)
        {
            Console.WriteLine($"Registration attempt for: {email}");
            
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View("~/Views/Accounts/Register.cshtml");
            }

            try
            {
                Console.WriteLine("Checking for existing user...");
                var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existing != null)
                {
                    Console.WriteLine($"User {email} already exists");
                    ViewBag.Error = "Email is already registered.";
                    return View("~/Views/Accounts/Register.cshtml");
                }

                Console.WriteLine("Creating new user...");
                // Always assign Customer role for new registrations
                var user = new User
                {
                    Email = email,
                    Password = password, // TODO: hash this in production
                    Role = "Customer" // Default role for all new registrations
                };

                _context.Users.Add(user);
                Console.WriteLine("Saving user to database...");
                await _context.SaveChangesAsync();
                Console.WriteLine($"User {email} saved successfully with ID: {user.Id}");

                // Auto-login after registration
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);
                Console.WriteLine($"Session set for user: {user.Email} with role: {user.Role}");

                // Route to customer dashboard since all new users are customers
                return RedirectToAction("Index", "CustomerIssues");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ViewBag.Error = $"Database error occurred: {ex.Message}";
                return View("~/Views/Accounts/Register.cshtml");
            }
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            return View("~/Views/Accounts/Login.cshtml");
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            Console.WriteLine($"Login attempt for: {email}");
            
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View("~/Views/Accounts/Login.cshtml");
            }

            try
            {
                Console.WriteLine("Attempting authentication...");
                var user = await _authService.AuthenticateAsync(email, password);

                if (user == null)
                {
                    Console.WriteLine($"Authentication failed for: {email}");
                    
                    // Let's also check if the user exists at all
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (existingUser == null)
                    {
                        Console.WriteLine($"User {email} does not exist in database");
                        ViewBag.Error = "User does not exist. Please register first.";
                    }
                    else
                    {
                        Console.WriteLine($"User {email} exists but password is incorrect");
                        ViewBag.Error = "Invalid email or password.";
                    }
                    return View("~/Views/Accounts/Login.cshtml");
                }

                Console.WriteLine($"Authentication successful for: {email}");

                // Set session
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);
                Console.WriteLine($"Session set for user: {user.Email} with role: {user.Role}");

                // Route to appropriate dashboard
                return RedirectToRoleDashboard(user.Role);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ViewBag.Error = $"Database error occurred: {ex.Message}";
                return View("~/Views/Accounts/Login.cshtml");
            }
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToRoleDashboard(string role)
        {
            return role switch
            {
                "Customer" => RedirectToAction("Index", "CustomerIssues"),
                "Engineer" => RedirectToAction("Index", "Engineer"),
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}
