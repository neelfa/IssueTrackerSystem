using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Controllers
{
    public class EngineerController : BaseController
    {
        private readonly AppDbContext _context;

        public EngineerController(AppDbContext context, IAuthService authService) : base(authService)
        {
            _context = context;
        }

        // Engineer Dashboard - Overview
        public async Task<IActionResult> Dashboard()
        {
            var authResult = RequireRole("Engineer");
            if (authResult is not EmptyResult) return authResult;

            var userEmail = GetCurrentUserEmail();

            // Get all issues statistics
            var totalIssues = await _context.Issues.CountAsync();
            var myAssignedIssues = await _context.Issues.CountAsync(i => i.AssignedToEmail == userEmail);
            var unassignedIssues = await _context.Issues.CountAsync(i => string.IsNullOrEmpty(i.AssignedToEmail));
            var inProgressIssues = await _context.Issues.CountAsync(i => i.Status == "InProgress" && i.AssignedToEmail == userEmail);
            var resolvedIssues = await _context.Issues.CountAsync(i => (i.Status == "Resolved" || i.Status == "Closed") && i.AssignedToEmail == userEmail);
            var overdueIssues = await _context.Issues.CountAsync(i => 
                (i.Status == "Open" || i.Status == "InProgress") && 
                i.AssignedToEmail == userEmail && 
                i.CreatedAt < DateTime.UtcNow.AddDays(-7));

            // Get recent assigned issues
            var recentAssignedIssues = await _context.Issues
                .Where(i => i.AssignedToEmail == userEmail)
                .OrderByDescending(i => i.UpdatedAt ?? i.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Get recent unassigned issues
            var recentUnassignedIssues = await _context.Issues
                .Where(i => string.IsNullOrEmpty(i.AssignedToEmail))
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Get priority breakdown for assigned issues
            var highPriorityCount = await _context.Issues.CountAsync(i => 
                i.AssignedToEmail == userEmail && i.Priority == "High" && 
                (i.Status == "Open" || i.Status == "InProgress"));
            var mediumPriorityCount = await _context.Issues.CountAsync(i => 
                i.AssignedToEmail == userEmail && i.Priority == "Medium" && 
                (i.Status == "Open" || i.Status == "InProgress"));
            var lowPriorityCount = await _context.Issues.CountAsync(i => 
                i.AssignedToEmail == userEmail && i.Priority == "Low" && 
                (i.Status == "Open" || i.Status == "InProgress"));

            var viewModel = new EngineerDashboardViewModel
            {
                TotalIssues = totalIssues,
                MyAssignedIssues = myAssignedIssues,
                UnassignedIssues = unassignedIssues,
                InProgressIssues = inProgressIssues,
                ResolvedIssues = resolvedIssues,
                OverdueIssues = overdueIssues,
                HighPriorityCount = highPriorityCount,
                MediumPriorityCount = mediumPriorityCount,
                LowPriorityCount = lowPriorityCount,
                RecentAssignedIssues = recentAssignedIssues,
                RecentUnassignedIssues = recentUnassignedIssues
            };

            return View(viewModel);
        }

        // Engineer Dashboard - All Issues
        public async Task<IActionResult> Index()
        {
            var authResult = RequireRole("Engineer");
            if (authResult is not EmptyResult) return authResult;

            var issues = await _context.Issues
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(issues);
        }

        // Engineer's Assigned Issues
        public async Task<IActionResult> Assigned()
        {
            var authResult = RequireRole("Engineer");
            if (authResult is not EmptyResult) return authResult;

            var userEmail = GetCurrentUserEmail();
            var issues = await _context.Issues
                .Where(i => i.AssignedToEmail == userEmail)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(issues);
        }

        // Issue Details for Engineer
        public async Task<IActionResult> Details(int id)
        {
            var authResult = RequireRole("Engineer");
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
                CanEdit = true, // Engineers can edit issues
                CanAssign = true,
                Engineers = engineers
            };

            return View(viewModel);
        }

        // Assign Issue to Engineer
        [HttpPost]
        public async Task<IActionResult> Assign(int id, string assignedToEmail)
        {
            var authResult = RequireRole("Engineer");
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

            return RedirectToAction(nameof(Details), new { id });
        }

        // Take Issue (assign to self)
        [HttpPost]
        public async Task<IActionResult> Take(int id)
        {
            var authResult = RequireRole("Engineer");
            if (authResult is not EmptyResult) return authResult;

            var userEmail = GetCurrentUserEmail();
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();

            issue.AssignedToEmail = userEmail;
            issue.UpdatedAt = DateTime.UtcNow;
            if (issue.Status == "Open")
            {
                issue.Status = "InProgress";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // Update Issue Status
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var authResult = RequireRole("Engineer");
            if (authResult is not EmptyResult) return authResult;

            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();

            var validStatuses = new[] { "Open", "InProgress", "Resolved", "Closed" };
            if (!validStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction(nameof(Details), new { id });
            }

            issue.Status = status;
            issue.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // Add comment/note
        [HttpPost]
        public async Task<IActionResult> AddComment(int issueId, string content)
        {
            var authResult = RequireRole("Engineer");
            if (authResult is not EmptyResult) return authResult;

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction(nameof(Details), new { id = issueId });
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

            return RedirectToAction(nameof(Details), new { id = issueId });
        }

        // API endpoint for chart data
        [HttpGet]
        public async Task<IActionResult> ChartData()
        {
            var authResult = RequireRole("Engineer");
            if (authResult is not EmptyResult) return authResult;

            var userEmail = GetCurrentUserEmail();

            // Basic metrics
            var totalIssues = await _context.Issues.CountAsync();
            var myAssignedIssues = await _context.Issues.CountAsync(i => i.AssignedToEmail == userEmail);
            var unassignedIssues = await _context.Issues.CountAsync(i => string.IsNullOrEmpty(i.AssignedToEmail));
            var resolvedIssues = await _context.Issues.CountAsync(i => (i.Status == "Resolved" || i.Status == "Closed") && i.AssignedToEmail == userEmail);
            var completionRate = myAssignedIssues > 0 ? Math.Round((double)resolvedIssues / myAssignedIssues * 100, 1) : 0;

            // Priority breakdown
            var highPriorityCount = await _context.Issues.CountAsync(i => 
                i.AssignedToEmail == userEmail && i.Priority == "High" && 
                (i.Status == "Open" || i.Status == "InProgress"));
            var mediumPriorityCount = await _context.Issues.CountAsync(i => 
                i.AssignedToEmail == userEmail && i.Priority == "Medium" && 
                (i.Status == "Open" || i.Status == "InProgress"));
            var lowPriorityCount = await _context.Issues.CountAsync(i => 
                i.AssignedToEmail == userEmail && i.Priority == "Low" && 
                (i.Status == "Open" || i.Status == "InProgress"));

            // Status breakdown
            var openIssues = await _context.Issues.CountAsync(i => i.Status == "Open" && i.AssignedToEmail == userEmail);
            var inProgressIssues = await _context.Issues.CountAsync(i => i.Status == "InProgress" && i.AssignedToEmail == userEmail);
            var overdueIssues = await _context.Issues.CountAsync(i => 
                (i.Status == "Open" || i.Status == "InProgress") && 
                i.AssignedToEmail == userEmail && 
                i.CreatedAt < DateTime.UtcNow.AddDays(-7));

            // Trend data - last 7 days
            var trendLabels = new List<string>();
            var createdData = new List<int>();
            var resolvedData = new List<int>();

            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                var nextDate = date.AddDays(1);
                
                trendLabels.Add(date.ToString("MMM dd"));
                
                var created = await _context.Issues.CountAsync(i => 
                    i.CreatedAt.Date == date && 
                    (string.IsNullOrEmpty(userEmail) || i.AssignedToEmail == userEmail));
                
                var resolved = await _context.Issues.CountAsync(i => 
                    i.UpdatedAt.HasValue && i.UpdatedAt.Value.Date == date && 
                    (i.Status == "Resolved" || i.Status == "Closed") && 
                    i.AssignedToEmail == userEmail);
                
                createdData.Add(created);
                resolvedData.Add(resolved);
            }

            var chartData = new
            {
                metrics = new
                {
                    totalIssues,
                    myIssues = myAssignedIssues,
                    available = unassignedIssues,
                    completionRate
                },
                priority = new
                {
                    high = highPriorityCount,
                    medium = mediumPriorityCount,
                    low = lowPriorityCount
                },
                status = new
                {
                    open = openIssues,
                    inProgress = inProgressIssues,
                    resolved = resolvedIssues,
                    overdue = overdueIssues
                },
                trend = new
                {
                    labels = trendLabels,
                    created = createdData,
                    resolved = resolvedData
                },
                performance = completionRate
            };

            return Json(chartData);
        }
    }
}