using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Service;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Policy = "RequireOwnerOrManager")]
    public class ManageStaffController : Controller
    {
        private readonly IManageStaffService _staffService;



        public ManageStaffController(IManageStaffService staffService)
        {
            _staffService = staffService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int branchId)
        {
            var listStaff = await _staffService.GetStaffByBranchAsync(branchId);
            ViewBag.Branches = branchId;
            return View(listStaff);
        }

        [HttpGet]
        public IActionResult Create(int branchId)
        {
            ViewBag.BranchId = branchId;
            ViewBag.Positions = Enum.GetValues(typeof(StaffRole));
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string username, string email, string password, StaffRole position, int branchId)
        {
            var result = await _staffService.CreateStaffAsync(username, email, password, position, branchId);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index), new { branchId });

            }
            return RedirectToAction(nameof(Index), new { branchId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, int branchId)
        {
            var result = await _staffService.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index), new { branchId });
            }

            ViewBag.BranchId = branchId;
            ViewBag.Positions = Enum.GetValues(typeof(StaffRole));
            return View(result.User);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int staffId, string username, StaffRole? position, int branchId)
        {
            var result = await _staffService.UpdateStaffAsync(staffId, username, position, branchId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Edit), new { id = staffId, branchId });
            }
            else
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index), new { branchId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int staffId, int branchId)
        {
            var result = await _staffService.DeleteStaffAsync(staffId, branchId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
            }
            else
            {
                TempData["SuccessMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index), new { branchId });
        }

    }
}






