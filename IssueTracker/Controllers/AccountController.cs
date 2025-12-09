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

        // GET: /Account/TestAdmin - temporary debugging endpoint
        [HttpGet]
        public async Task<IActionResult> TestAdmin()
        {
            try
            {
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com");
                if (adminUser == null)
                {
                    return Json(new { found = false, message = "Admin user not found in database" });
                }
                
                return Json(new { 
                    found = true, 
                    email = adminUser.Email, 
                    role = adminUser.Role,
                    roleLength = adminUser.Role?.Length,
                    roleBytes = System.Text.Encoding.UTF8.GetBytes(adminUser.Role ?? "").Select(b => (int)b).ToArray()
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
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
                        Console.WriteLine($"Database role: '{existingUser.Role}'");
                        ViewBag.Error = "Invalid email or password.";
                    }
                    return View("~/Views/Accounts/Login.cshtml");
                }

                Console.WriteLine($"Authentication successful for: {email}");
                Console.WriteLine($"User role from database: '{user.Role}'");
                Console.WriteLine($"User role length: {user.Role?.Length}");

                // Clear any existing session first
                HttpContext.Session.Clear();

                // Set session with explicit logging
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);
                Console.WriteLine($"Session set - Email: '{HttpContext.Session.GetString("UserEmail")}'");
                Console.WriteLine($"Session set - Role: '{HttpContext.Session.GetString("UserRole")}'");

                // Route to appropriate dashboard
                var redirectResult = RedirectToRoleDashboard(user.Role);
                Console.WriteLine($"Redirect result: {redirectResult}");
                return redirectResult;
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

        // GET: /Account/FixAdmin - temporary debugging endpoint to fix admin role
        [HttpGet]
        public async Task<IActionResult> FixAdmin()
        {
            try
            {
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com");
                if (adminUser == null)
                {
                    return Json(new { success = false, message = "Admin user not found in database" });
                }
                
                var oldRole = adminUser.Role;
                adminUser.Role = "Admin";
                await _context.SaveChangesAsync();
                
                return Json(new { 
                    success = true, 
                    message = $"Admin role updated from '{oldRole}' to '{adminUser.Role}'" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // GET: /Account/ResetTestUsers - reset all test users with correct roles
        [HttpGet]
        public async Task<IActionResult> ResetTestUsers()
        {
            try
            {
                // Find and update/create test users
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com");
                var engineerUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "engineer@test.com");
                var customerUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "customer@test.com");

                if (adminUser != null)
                {
                    adminUser.Role = "Admin";
                }
                else
                {
                    _context.Users.Add(new User { Email = "admin@test.com", Password = "admin123", Role = "Admin" });
                }

                if (engineerUser != null)
                {
                    engineerUser.Role = "Engineer";
                }
                else
                {
                    _context.Users.Add(new User { Email = "engineer@test.com", Password = "engineer123", Role = "Engineer" });
                }

                if (customerUser != null)
                {
                    customerUser.Role = "Customer";
                }
                else
                {
                    _context.Users.Add(new User { Email = "customer@test.com", Password = "customer123", Role = "Customer" });
                }

                await _context.SaveChangesAsync();
                
                return Json(new { 
                    success = true, 
                    message = "All test users have been reset with correct roles" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private IActionResult RedirectToRoleDashboard(string role)
        {
            Console.WriteLine($"RedirectToRoleDashboard called with role: '{role}' (length: {role?.Length})");
            
            // Trim whitespace and normalize the role
            var normalizedRole = role?.Trim();
            Console.WriteLine($"Normalized role: '{normalizedRole}'");
            
            var result = normalizedRole switch
            {
                "Customer" => RedirectToAction("Index", "CustomerIssues"),
                "Engineer" => RedirectToAction("Index", "Engineer"),
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                _ => RedirectToAction("Index", "Home")
            };
            
            Console.WriteLine($"Redirecting {normalizedRole} to action result: {result}");
            return result;
        }
    }
}
