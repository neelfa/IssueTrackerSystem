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
            return HttpContext.Session.GetString("UserRole");
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
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }
            
            var userRole = GetCurrentUserRole();
            if (userRole != "Admin" && !roles.Contains(userRole))
            {
                return View("~/Views/Shared/Unauthorized.cshtml");
            }
            
            return new EmptyResult();
        }
    }
}