using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Controllers
{
    public class CustomerIssuesController : Controller
    {
        private readonly AppDbContext _context;

        public CustomerIssuesController(AppDbContext context)
        {
            _context = context;
        }

        // READ: list issues for customer
        public async Task<IActionResult> Index()
        {
            // later you can filter by logged‑in user; for now show all
            var issues = await _context.Issues
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            // will use Views/CustomerIssues/Index.cshtml
            return View(issues);
        }

        // READ: details
        public async Task<IActionResult> Details(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();
            return View(issue);
        }

        // CREATE (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // CREATE (POST)
        [HttpPost]
        public async Task<IActionResult> Create(Issue issue)
        {
            if (!ModelState.IsValid)
                return View(issue);

            issue.CreatedAt = DateTime.UtcNow;
            issue.Status = "Open";
            // later: set CreatedByEmail from logged‑in user
            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // UPDATE (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();
            return View(issue);
        }

        // UPDATE (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(Issue issue)
        {
            if (!ModelState.IsValid)
                return View(issue);

            issue.UpdatedAt = DateTime.UtcNow;
            _context.Issues.Update(issue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE (GET)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();
            return View(issue);
        }

        // DELETE (POST)
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var issue = await _context.Issues.FindAsync(id);
            if (issue == null) return NotFound();

            _context.Issues.Remove(issue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
