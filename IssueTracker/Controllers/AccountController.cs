using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            // Views/Accounts/Register.cshtml
            return View("~/Views/Accounts/Register.cshtml");
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View("~/Views/Accounts/Register.cshtml");
            }

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existing != null)
            {
                ViewBag.Error = "Email is already registered.";
                return View("~/Views/Accounts/Register.cshtml");
            }

            var user = new User
            {
                Email = email,
                Password = password, // later: hash this
                Role = string.IsNullOrEmpty(role) ? "Customer" : role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // after registration go to login
            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Views/Accounts/Login.cshtml
            return View("~/Views/Accounts/Login.cshtml");
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View("~/Views/Accounts/Login.cshtml");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View("~/Views/Accounts/Login.cshtml");
            }

            // Route based on role
            if (user.Role == "Customer")
                // go to the working CRUD page
                return RedirectToAction("Index", "CustomerIssues");

            if (user.Role == "Engineer")
                return RedirectToAction("Index", "Engineer");

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Admin");

            // fallback
            return RedirectToAction("Index", "Home");
        }
    }
}
