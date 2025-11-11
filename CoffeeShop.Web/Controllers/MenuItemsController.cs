using System.Security.Claims;
using CoffeeShop.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Policy = "RequireOwnerOrManager")]
    public class MenuItemsController : Controller
    {
        private readonly IMenuItemService _menuItemService;
        private readonly IAuthService _authService;

        public MenuItemsController(IMenuItemService menuItemService, IAuthService authService)
        {
            _menuItemService = menuItemService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? branchId, string? category = null)
        {
            int resolvedBranchId;

            // ✅ Nếu Owner truyền branchId thì dùng, nếu không thì lấy từ Claim của Manager
            if (branchId.HasValue && branchId.Value > 0)
                resolvedBranchId = branchId.Value;
            else
            {
                var branchIdClaim = User.FindFirst("BranchId")?.Value;
                if (string.IsNullOrEmpty(branchIdClaim))
                {
                    TempData["ErrorMessage"] = "No branch assigned for this account.";
                    return RedirectToAction("Forbidden", "Auth");
                }
                resolvedBranchId = int.Parse(branchIdClaim);
            }

            // ✅ Dùng resolvedBranchId cho tất cả service
            var menuItems = await _menuItemService.GetByCategoryAsync(resolvedBranchId, category);
            var categories = await _menuItemService.GetCategoriesAsync(resolvedBranchId);

            ViewBag.BranchId = resolvedBranchId;
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;

            return View(menuItems);
        }

        [HttpGet]
        public IActionResult Create(int? branchId)
        {
            int resolvedBranchId = branchId ?? int.Parse(User.FindFirst("BranchId")!.Value);
            ViewBag.BranchId = resolvedBranchId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? branchId, string name, decimal price, string? category, IFormFile? imageFile, bool isAvailable = true)
        {
            int resolvedBranchId = branchId ?? int.Parse(User.FindFirst("BranchId")!.Value);

            var result = await _menuItemService.CreateAsync(resolvedBranchId, name, price, category, imageFile, isAvailable);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index), new { branchId = resolvedBranchId });
            }

            TempData["ErrorMessage"] = result.Message;
            ViewBag.BranchId = resolvedBranchId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _menuItemService.GetByIdAsync(id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            ViewBag.BranchId = result.MenuItem.BranchId;
            return View(result.MenuItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int menuItemId, string name, decimal price, string? category, IFormFile? imageFile, bool isAvailable, int? branchId)
        {
            int resolvedBranchId = branchId ?? int.Parse(User.FindFirst("BranchId")!.Value);

            var result = await _menuItemService.UpdateAsync(menuItemId, name, price, category, imageFile, isAvailable, resolvedBranchId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index), new { branchId = resolvedBranchId });
            }

            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Edit), new { id = menuItemId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _menuItemService.DeleteAsync(id);

            int resolvedBranchId = result.MenuItem?.BranchId ?? int.Parse(User.FindFirst("BranchId")!.Value);

            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index), new { branchId = resolvedBranchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var result = await _menuItemService.ToggleAvailabilityAsync(id);

            int resolvedBranchId = result.MenuItem?.BranchId ?? int.Parse(User.FindFirst("BranchId")!.Value);

            TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index), new { branchId = resolvedBranchId });
        }
    }
}
