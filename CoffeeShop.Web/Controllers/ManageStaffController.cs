using System.Security.Claims;
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
        private readonly IAuthService _authService;

        public ManageStaffController(IManageStaffService staffService, IAuthService authService)
        {
            _staffService = staffService;
            _authService = authService;
        }

        // =========================
        // LIST
        // =========================
        [HttpGet]
        public async Task<IActionResult> Index(int? branchId)
        {
            var me = await _authService.GetCurrentUserAsync();
            if (me == null) return RedirectToAction("Login", "Auth");

            // OWNER: xem theo branch nếu có, không thì xem tất cả trong Business
            if (me.Role == UserRole.Owner)
            {
                if (branchId.HasValue && branchId.Value > 0)
                {
                    var listByBranch = await _staffService.GetStaffByBranchAsync(branchId.Value);
                    ViewBag.BranchMode = true;
                    ViewBag.BranchId = branchId.Value;
                    return View(listByBranch);
                }

                if (!me.BusinessId.HasValue)
                {
                    TempData["ErrorMessage"] = "Your owner account has no Business assigned.";
                    return RedirectToAction("My", "Business");
                }

                var all = await _staffService.GetAllStaffAsync(me.BusinessId.Value);
                ViewBag.BranchMode = false;
                return View(all);
            }

            // MANAGER: luôn khóa theo chi nhánh của chính mình
            if (me.Role == UserRole.Staff &&
                me.StaffProfile?.Position == StaffRole.Manager &&
                me.BranchId.HasValue)
            {
                var managerBranchId = me.BranchId.Value;

                // Nếu có ai cố truyền branchId khác -> đưa về đúng chi nhánh manager
                if (branchId.HasValue && branchId.Value != managerBranchId)
                    return RedirectToAction(nameof(Index), new { branchId = managerBranchId });

                var list = await _staffService.GetStaffByBranchAsync(managerBranchId);
                ViewBag.BranchMode = true;
                ViewBag.BranchId = managerBranchId;
                return View(list);
            }

            return Forbid();
        }

        // =========================
        // CREATE
        // =========================
        [HttpGet]
        public async Task<IActionResult> Create(int? branchId)
        {
            var me = await _authService.GetCurrentUserAsync();
            if (me == null) return RedirectToAction("Login", "Auth");

            int resolvedBranchId;

            if (me.Role == UserRole.Owner)
            {
                if (!branchId.HasValue || branchId.Value <= 0)
                {
                    TempData["ErrorMessage"] = "Please choose a branch first.";
                    return RedirectToAction("Index", "Branch");
                }
                resolvedBranchId = branchId.Value;
            }
            else
            {
                // MANAGER: luôn lấy theo claim, bỏ qua tham số ngoài
                if (!me.BranchId.HasValue) return Forbid();
                resolvedBranchId = me.BranchId.Value;
            }

            ViewBag.BranchId = resolvedBranchId;
            ViewBag.Positions = Enum.GetValues(typeof(StaffRole));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string username, string email, string password, StaffRole position, int branchId)
        {
            var me = await _authService.GetCurrentUserAsync();
            if (me == null) return RedirectToAction("Login", "Auth");

            // MANAGER: ép branchId = chi nhánh của mình để không thể tạo cho chi nhánh khác
            if (me.Role != UserRole.Owner)
            {
                if (!me.BranchId.HasValue) return Forbid();
                branchId = me.BranchId.Value;
            }

            var result = await _staffService.CreateStaffAsync(username, email, password, position, branchId);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index), new { branchId });
        }

        // =========================
        // EDIT
        // =========================
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int branchId)
        {
            var me = await _authService.GetCurrentUserAsync();
            if (me == null) return RedirectToAction("Login", "Auth");

            var res = await _staffService.GetByIdAsync(id);
            if (!res.IsSuccess || res.User == null)
            {
                TempData["ErrorMessage"] = res.Message;
                return RedirectToAction(nameof(Index), new { branchId });
            }

            // MANAGER: chỉ được sửa nhân viên trong chi nhánh của mình
            if (me.Role == UserRole.Staff &&
                me.StaffProfile?.Position == StaffRole.Manager &&
                me.BranchId.HasValue &&
                res.User.BranchId != me.BranchId.Value)
            {
                return Forbid();
            }

            ViewBag.BranchId = res.User.BranchId ?? branchId;
            ViewBag.Positions = Enum.GetValues(typeof(StaffRole));
            return View(res.User);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int staffId, string username, StaffRole? position, int branchId)
        {
            var me = await _authService.GetCurrentUserAsync();
            if (me == null) return RedirectToAction("Login", "Auth");

            // MANAGER: bắt buộc branchId theo claim
            if (me.Role != UserRole.Owner)
            {
                if (!me.BranchId.HasValue) return Forbid();
                branchId = me.BranchId.Value;
            }

            var result = await _staffService.UpdateStaffAsync(staffId, username, position, branchId);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index), new { branchId });
        }

        // =========================
        // DELETE
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int staffId, int branchId)
        {
            var me = await _authService.GetCurrentUserAsync();
            if (me == null) return RedirectToAction("Login", "Auth");

            // MANAGER: ép branchId theo claim
            if (me.Role != UserRole.Owner)
            {
                if (!me.BranchId.HasValue) return Forbid();
                branchId = me.BranchId.Value;
            }

            var result = await _staffService.DeleteStaffAsync(staffId, branchId);
            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index), new { branchId });
        }
    }
}
