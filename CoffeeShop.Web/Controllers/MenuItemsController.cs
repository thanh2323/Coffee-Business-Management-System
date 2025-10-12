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
        public async Task<IActionResult> Index(int branchId, string? category = null)
        {
           
            var menuItems = await _menuItemService.GetByCategoryAsync(branchId, category);
         
            var categories = await _menuItemService.GetCategoriesAsync(branchId);

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
           
            var result = await _menuItemService.CreateAsync( branchId, name, price, category, isAvailable);

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
         
            var result = await _menuItemService.GetByIdAsync( id);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }

            return View(result.MenuItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int menuItemId, string name, decimal price, string? category, bool isAvailable, int branchId)
            {
            
            var result = await _menuItemService.UpdateAsync(menuItemId, name, price, category, isAvailable, branchId);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction(nameof(Edit), new { branchId });
            }
            else
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index), new { branchId });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
           
            var result = await _menuItemService.DeleteAsync(id);

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
            var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claimUserId == null || !int.TryParse(claimUserId, out var userId))
            {
                TempData["ErrorMessage"] = "Invalid user.";
                return RedirectToAction("Index", "Home");
            }
            var result = await _menuItemService.ToggleAvailabilityAsync(id);


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

