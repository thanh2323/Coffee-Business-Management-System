using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Policy = "RequireManager")]
    public class ManagerController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var branchIdClaim = User.FindFirst("BranchId");
            if (branchIdClaim == null || !int.TryParse(branchIdClaim.Value, out var branchId))
            {
                TempData["Error"] = "Branch context not found.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.BranchId = branchId;
            return View();
        }

        [HttpGet]
        public IActionResult SwitchToBranch()
        {
            var branchIdClaim = User.FindFirst("BranchId");
            if (branchIdClaim == null || !int.TryParse(branchIdClaim.Value, out var branchId))
            {
                TempData["Error"] = "Branch context not found.";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Branch", new { id = branchId });
        }
    }
}




