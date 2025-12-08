using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Controllers
{
    public class CustomerIssuesController : BaseController
    {
        private readonly AppDbContext _context;

        public CustomerIssuesController(AppDbContext context, IAuthService authService) : base(authService)
        {
            _context = context;
        }

        // READ: list issues for customer
        public async Task<IActionResult> Index()
        {
            var authResult = RequireRole("Customer");
            if (authResult is not EmptyResult) return authResult;

            var userEmail = GetCurrentUserEmail();
            var issues = await _context.Issues
                .Where(i => i.CreatedByEmail == userEmail)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(issues);
        }

        // READ: details with comments
        public async Task<IActionResult> Details(int id)
        {
            var authResult = RequireRole("Customer");
            if (authResult is not EmptyResult) return authResult;

            var userEmail = GetCurrentUserEmail();
            var issue = await _context.Issues
                .Include(i => i.Comments)
                .FirstOrDefaultAsync(i => i.Id == id && i.CreatedByEmail == userEmail);
                
            if (issue == null) return NotFound();

            var viewModel = new IssueDetailViewModel
            {
                Issue = issue,
                Comments = issue.Comments.OrderBy(c => c.CreatedAt).ToList(),
                CanEdit = true, // Customer can edit their own issues
                CanAssign = false
            };

            return View(viewModel);
        }

        // CREATE (GET)
        [HttpGet]
        public IActionResult Create()
        {
            var authResult = RequireRole("Customer");
            if (authResult is not EmptyResult) return authResult;

            return View(new CreateIssueViewModel());
        }

        // CREATE (POST)
        [HttpPost]
        public async Task<IActionResult> Create(CreateIssueViewModel model)
        {
            var authResult = RequireRole("Customer");
            if (authResult is not EmptyResult) return authResult;

            if (!ModelState.IsValid)
                return View(model);

            var issue = new Issue
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                CreatedAt = DateTime.UtcNow,
                Status = "Open",
                CreatedByEmail = GetCurrentUserEmail()!
            };

            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // UPDATE (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var authResult = RequireRole("Customer");
            if (authResult is not EmptyResult) return authResult;

            var userEmail = GetCurrentUserEmail();
            var issue = await _context.Issues
                .FirstOrDefaultAsync(i => i.Id == id && i.CreatedByEmail == userEmail);
                
            if (issue == null) return NotFound();

            // Customer can only edit if issue is still Open
            if (issue.Status != "Open")
            {
                TempData["Error"] = "You can only edit issues that are still open.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(issue);
        }

        // UPDATE (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(Issue issue)
        {
            var authResult = RequireRole("Customer");
            if (authResult is not EmptyResult) return authResult;

            var userEmail = GetCurrentUserEmail();
            var existingIssue = await _context.Issues
                .FirstOrDefaultAsync(i => i.Id == issue.Id && i.CreatedByEmail == userEmail);
                
            if (existingIssue == null) return NotFound();

            if (existingIssue.Status != "Open")
            {
                TempData["Error"] = "You can only edit issues that are still open.";
                return RedirectToAction(nameof(Details), new { id = issue.Id });
            }

            // Update only allowed fields
            existingIssue.Title = issue.Title;
            existingIssue.Description = issue.Description;
            existingIssue.Priority = issue.Priority;
            existingIssue.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = issue.Id });
        }

        // Close issue (Customer can close their own issues)
        [HttpPost]
        public async Task<IActionResult> Close(int id)
        {
            var authResult = RequireRole("Customer");
            if (authResult is not EmptyResult) return authResult;

            var userEmail = GetCurrentUserEmail();
            var issue = await _context.Issues
                .FirstOrDefaultAsync(i => i.Id == id && i.CreatedByEmail == userEmail);
                
            if (issue == null) return NotFound();

            issue.Status = "Closed";
            issue.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // Add comment
        [HttpPost]
        public async Task<IActionResult> AddComment(int issueId, string content)
        {
            var authResult = RequireRole("Customer");
            if (authResult is not EmptyResult) return authResult;

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Comment cannot be empty.";
                return RedirectToAction(nameof(Details), new { id = issueId });
            }

            var userEmail = GetCurrentUserEmail();
            var issue = await _context.Issues
                .FirstOrDefaultAsync(i => i.Id == issueId && i.CreatedByEmail == userEmail);
                
            if (issue == null) return NotFound();

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
    }
}