using System.Security.Claims;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Roles = "Owner")]
    public class BranchController : Controller
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        private bool TryGetOwnerBusinessId(out int businessId)
        {
            businessId = 0;
            var claim = User.FindFirst("BusinessId")?.Value;
            return claim != null && int.TryParse(claim, out businessId);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var ownerId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("My", "Business");
            }

            var branches = await _branchService.GetBranchesForOwnerAsync(ownerId);
            var ctx = await _branchService.GetOwnerContextAsync(ownerId);
            ViewBag.BusinessName = ctx.businessName;
            ViewBag.OwnerName = ctx.ownerName;
            return View(branches);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var ownerId))
            {
                TempData["Error"] = "Invalid user.";
                return View();
            }
            var ctx = await _branchService.GetOwnerContextAsync(ownerId);
            ViewBag.BusinessName = ctx.businessName;
            ViewBag.OwnerName = ctx.ownerName;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, string? address, TimeSpan openTime, TimeSpan closeTime)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var ownerId))
            {
                TempData["Error"] = "Invalid user.";
                return View();
            }

            var result = await _branchService.CreateBranchAsync(ownerId, name, address, openTime, closeTime);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View();
            }
            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (ownerIdClaim == null || !int.TryParse(ownerIdClaim, out var ownerId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("Index");
            }

            // Load via owner list to ensure ownership
            var branches = await _branchService.GetBranchesForOwnerAsync(ownerId);
            var ctx = await _branchService.GetOwnerContextAsync(ownerId);
            ViewBag.BusinessName = ctx.businessName;
            ViewBag.OwnerName = ctx.ownerName;
            var branch = branches.FirstOrDefault(b => b.BranchId == id);
            if (branch == null)
            {
                TempData["Error"] = "Branch not found.";
                return RedirectToAction("Index");
            }
            return View(branch);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, string name, string? address, TimeSpan openTime, TimeSpan closeTime)
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (ownerIdClaim == null || !int.TryParse(ownerIdClaim, out var ownerId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("Index");
            }

            var result = await _branchService.UpdateBranchAsync(ownerId, id, name, address, openTime, closeTime);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                // best-effort: reload branch for view
                var branches = await _branchService.GetBranchesForOwnerAsync(ownerId);
                var branch = branches.FirstOrDefault(b => b.BranchId == id) ?? new Branch();
                return View(branch);
            }
            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (ownerIdClaim == null || !int.TryParse(ownerIdClaim, out var ownerId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("Index");
            }

            var result = await _branchService.DeleteBranchAsync(ownerId, id);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index");
        }
    }
}


