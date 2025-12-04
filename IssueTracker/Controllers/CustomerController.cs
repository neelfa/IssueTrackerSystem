using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.Controllers
{
    public class CustomerController : Controller
    {
        // GET: /Customer/Issues
        public IActionResult Issues()
        {
            return View();   // will look for Views/Customer/Issues.cshtml
        }
    }
}
