using CoffeeShop.Application.Interface.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Policy = "RequireOwnerOrManager")]
    public class MenuItemsController : Controller
    {
        private readonly IMenuItemService _menuItemService;

        public MenuItemsController(IMenuItemService menuItemService)
        {
            _menuItemService = menuItemService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int branchId, string? category = null)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var menuItems = await _menuItemService.GetByCategoryAsync(userId, branchId, category);
            var categories = await _menuItemService.GetCategoriesAsync(userId, branchId);

            ViewBag.BranchId = branchId;
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;

            return View(menuItems);
        }

        [HttpGet]
        public IActionResult Create(int branchId)
        {
            ViewBag.BranchId = branchId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int branchId, string name, decimal price, string? category, bool isAvailable = true)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var result = await _menuItemService.CreateAsync(userId, branchId, name, price, category, isAvailable);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index), new { branchId });
            }

            TempData["ErrorMessage"] = result.Message;
            ViewBag.BranchId = branchId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var result = await _menuItemService.GetByIdAsync(userId, id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.MenuItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int menuItemId, string name, decimal price, string? category, bool isAvailable)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var result = await _menuItemService.UpdateAsync(userId, menuItemId, name, price, category, isAvailable);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index), new { branchId = result.MenuItem!.BranchId });
            }

            TempData["ErrorMessage"] = result.Message;
            return View(result.MenuItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var result = await _menuItemService.DeleteAsync(userId, id);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index), new { branchId = result.MenuItem?.BranchId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var result = await _menuItemService.ToggleAvailabilityAsync(userId, id);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index), new { branchId = result.MenuItem?.BranchId });
        }
    }
}

