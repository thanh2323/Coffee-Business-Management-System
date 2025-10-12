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
        private readonly IAuthService _authService;

        public BranchController(IBranchService branchService, IAuthService authService)
        {
            _branchService = branchService;
            _authService = authService;

        }

        

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("My", "Business");
            }

            var branches = await _branchService.GetBranchesForOwnerAsync(currentUser.UserId);
            var ctx = await _branchService.GetOwnerContextAsync(currentUser.UserId);
            ViewBag.BusinessName = ctx.businessName;
            ViewBag.OwnerName = ctx.ownerName;
            return View(branches);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("My", "Business");
            }
            var ctx = await _branchService.GetOwnerContextAsync(currentUser.UserId);
            ViewBag.BusinessName = ctx.businessName;
            ViewBag.OwnerName = ctx.ownerName;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name, string? address, TimeSpan openTime, TimeSpan closeTime)
        {
            var currentUser =  await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("My", "Business");
            }

            var result = await _branchService.CreateBranchAsync(currentUser.UserId, name, address, openTime, closeTime);
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
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("My", "Business");
            }
            // Load via owner list to ensure ownership
            var branches = await _branchService.GetBranchesForOwnerAsync(currentUser.UserId);
            var ctx = await _branchService.GetOwnerContextAsync(currentUser.UserId);
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
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("My", "Business");
            }

            var result = await _branchService.UpdateBranchAsync(currentUser.UserId, id, name, address, openTime, closeTime);
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                // best-effort: reload branch for view
                var branches = await _branchService.GetBranchesForOwnerAsync(currentUser.UserId);
                var branch = branches.FirstOrDefault(b => b.BranchId == id) ?? new Branch();
                return View(branch);
            }
            TempData["Success"] = result.Message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("My", "Business");
            }


            var result = await _branchService.DeleteBranchAsync(currentUser.UserId, id);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index");
        }
    }
}


