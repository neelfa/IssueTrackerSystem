using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace IssueTracker.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                // issue stats
                TotalIssues = await _context.Issues.CountAsync(),
                OpenIssues = await _context.Issues.CountAsync(i => i.Status == "Open"),
                InProgressIssues = await _context.Issues.CountAsync(i => i.Status == "InProgress"),
                ClosedIssues = await _context.Issues.CountAsync(i => i.Status == "Resolved" || i.Status == "Closed"),

                // user stats
                CustomersCount = await _context.Users.CountAsync(u => u.Role == "Customer"),
                EngineersCount = await _context.Users.CountAsync(u => u.Role == "Engineer"),
                AdminsCount = await _context.Users.CountAsync(u => u.Role == "Admin"),

                // recent issues
                RecentIssues = await _context.Issues
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model); // Views/Admin/Dashboard.cshtml
        }

        // GET: /Admin/Issues
        public async Task<IActionResult> Issues()
        {
            var issues = await _context.Issues
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(issues); // Views/Admin/Issues.cshtml
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            return View(users); // Views/Admin/Users.cshtml
        }

        // POST: /Admin/ChangeRole
        [HttpPost]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Role = role;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Users));
        }
    }
}
