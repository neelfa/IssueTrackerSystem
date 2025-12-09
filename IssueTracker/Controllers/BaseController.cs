using Microsoft.AspNetCore.Mvc;
using IssueTracker.Services;

namespace IssueTracker.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IAuthService _authService;
        
        public BaseController(IAuthService authService)
        {
            _authService = authService;
        }
        
        protected string? GetCurrentUserEmail()
        {
            return HttpContext.Session.GetString("UserEmail");
        }
        
        protected string? GetCurrentUserRole()
        {
            var role = HttpContext.Session.GetString("UserRole");
            Console.WriteLine($"GetCurrentUserRole called - returning: '{role}'");
            return role;
        }
        
        protected bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(GetCurrentUserEmail());
        }
        
        protected bool IsInRole(string role)
        {
            var userRole = GetCurrentUserRole();
            return userRole == role || userRole == "Admin"; // Admin has access to all roles
        }
        
        protected IActionResult RequireAuthentication()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }
            return new EmptyResult();
        }
        
        protected IActionResult RequireRole(params string[] roles)
        {
            Console.WriteLine($"RequireRole called with roles: [{string.Join(", ", roles)}]");
            
            if (!IsAuthenticated())
            {
                Console.WriteLine("User not authenticated, redirecting to login");
                return RedirectToAction("Login", "Account");
            }
            
            var userRole = GetCurrentUserRole();
            Console.WriteLine($"Current user role from session: '{userRole}'");
            
            if (userRole != "Admin" && !roles.Contains(userRole))
            {
                Console.WriteLine($"Access denied. User role '{userRole}' not in required roles [{string.Join(", ", roles)}]");
                return View("~/Views/Shared/Unauthorized.cshtml");
            }
            
            Console.WriteLine($"Access granted for role '{userRole}'");
            return new EmptyResult();
        }
    }
}