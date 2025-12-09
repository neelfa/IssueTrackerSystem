using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace IssueTracker.Controllers
{
    public class AdminController : BaseController
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context, IAuthService authService) : base(authService)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var authResult = RequireRole("Admin");
            if (authResult is not EmptyResult) return authResult;

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

            return View(model);
        }

        // GET: /Admin/ExportReport
        public async Task<IActionResult> ExportReport()
        {
            var authResult = RequireRole("Admin");
            if (authResult is not EmptyResult) return authResult;

            try
            {
                var issues = await _context.Issues
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();

                var csv = new StringBuilder();
                csv.AppendLine("ID,Title,Status,Priority,Created By,Created Date,Updated Date");

                foreach (var issue in issues)
                {
                    csv.AppendLine($"{issue.Id},{issue.Title?.Replace(",", ";")}," +
                                  $"{issue.Status},{issue.Priority},{issue.CreatedByEmail}," +
                                  $"{issue.CreatedAt:yyyy-MM-dd},{issue.UpdatedAt?.ToString("yyyy-MM-dd") ?? ""}");
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                var fileName = $"IssueTracker_Report_{DateTime.Now:yyyyMMdd}.csv";

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to export report: {ex.Message}";
                return RedirectToAction(nameof(Dashboard));
            }
        }

        // GET: /Admin/Issues (Admin can see all issues)
        public async Task<IActionResult> Issues()
        {
            var authResult = RequireRole("Admin");
            if (authResult is not EmptyResult) return authResult;

            var issues = await _context.Issues
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(issues);
        }

        // Issue Details for Admin (same as Engineer but with admin privileges)
        public async Task<IActionResult> IssueDetails(int id)
        {
            var authResult = RequireRole("Admin");
            if (authResult is not EmptyResult) return authResult;

            var issue = await _context.Issues
                .Include(i => i.Comments)
                .FirstOrDefaultAsync(i => i.Id == id);
                
            if (issue == null) return NotFound();

            var engineers = await _authService.GetUsersByRoleAsync("Engineer");

            var viewModel = new IssueDetailViewModel
            {
                Issue = issue,
                Comments = issue.Comments.OrderBy(c => c.CreatedAt).ToList(),
                CanEdit = false, // Admin can only view, not edit
                CanAssign = false, // Admin cannot assign
                Engineers = engineers
            };

            return View("Details", viewModel); // Now uses Admin/Details.cshtml
        }

        // Admin Details action - routes to the same logic as IssueDetails (View Only)
        public async Task<IActionResult> Details(int id)
        {
            return await IssueDetails(id);
        }

        // GET: /Admin/Users (User Management)
        public async Task<IActionResult> Users()
        {
            var authResult = RequireRole("Admin");
            if (authResult is not EmptyResult) return authResult;

            var users = await _context.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Email)
                .ToListAsync();

            var viewModel = new UserManagementViewModel
            {
                Users = users
            };

            return View(viewModel);
        }

        // POST: /Admin/ChangeRole
        [HttpPost]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            var authResult = RequireRole("Admin");
            if (authResult is not EmptyResult) return authResult;

            var validRoles = new[] { "Customer", "Engineer", "Admin" };
            if (!validRoles.Contains(role))
            {
                TempData["Error"] = "Invalid role selected.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            // Prevent admins from changing their own role (to avoid lockout)
            var currentUserEmail = GetCurrentUserEmail();
            if (user.Email == currentUserEmail)
            {
                TempData["Error"] = "You cannot change your own role.";
                return RedirectToAction(nameof(Users));
            }

            user.Role = role;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Role updated successfully for {user.Email}.";
            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var authResult = RequireRole("Admin");
            if (authResult is not EmptyResult) return authResult;

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            // Prevent admins from deleting themselves
            var currentUserEmail = GetCurrentUserEmail();
            if (user.Email == currentUserEmail)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User {user.Email} deleted successfully.";
            return RedirectToAction(nameof(Users));
        }

        // Admin can do everything Engineers can do for issues
        /* DISABLED - Admin Details is now view-only
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var authResult = RequireRole("Admin");
            if (authResult is not EmptyResult) return authResult;

            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();

            var validStatuses = new[] { "Open", "InProgress", "Resolved", "Closed" };
            if (!validStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction(nameof(IssueDetails), new { id });
            }

            issue.Status = status;
            issue.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IssueDetails), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Assign(int id, string assignedToEmail)
        {
            var authResult = RequireRole("Admin");
            if (authResult is not EmptyResult) return authResult;

            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();

            issue.AssignedToEmail = assignedToEmail;
            issue.UpdatedAt = DateTime.UtcNow;
            if (issue.Status == "Open")
            {
                issue.Status = "InProgress";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IssueDetails), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int issueId, string content)
        {
            var authResult = RequireRole("Admin");
            if (authResult is not EmptyResult) return authResult;

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction(nameof(IssueDetails), new { id = issueId });
            }

            var userEmail = GetCurrentUserEmail();
            var comment = new Comment
            {
                Content = content,
                CreatedAt = DateTime.UtcNow,
                CreatedByEmail = userEmail!,
                IssueId = issueId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(IssueDetails), new { id = issueId });
        }
        */
    }
}
