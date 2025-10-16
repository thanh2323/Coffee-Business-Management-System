using CoffeeShop.Application.Interface.IService;
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
    }
}






