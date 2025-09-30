using System.Security.Claims;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Policy = "RequireOwnerOrManager")]
    public class IngredientsController : Controller
    {
        private readonly IIngredientService _ingredientService;

        public IngredientsController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null && int.TryParse(claim, out userId);
        }

        [HttpGet]
        public async Task<IActionResult> Index(int branchId)
        {
            if (!TryGetUserId(out var userId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("Index", "Home");
            }

            var items = await _ingredientService.GetByBranchAsync(userId, branchId);
            ViewBag.BranchId = branchId;
            return View(items);
        }

        [HttpGet]
        public IActionResult Create(int branchId)
        {
            ViewBag.BranchId = branchId;
            ViewBag.BaseUnits = Enum.GetValues(typeof(BaseUnit));
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            int branchId,
            string name,
            decimal quantity,
            decimal unitCost,
            string? displayUnit)
        {
            if (!TryGetUserId(out var userId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _ingredientService.CreateAsync(userId, branchId, name, quantity, unitCost, displayUnit);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index", new { branchId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int branchId, string ingredientName)
        {
            if (!TryGetUserId(out var userId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("Index", "Home");
            }

            var list = await _ingredientService.GetByBranchAsync(userId, branchId);
            var ing = list.FirstOrDefault(i => i.Name == ingredientName);
            if (ing == null)
            {
                TempData["Error"] = "Ingredient not found";
                return RedirectToAction("Index", new { branchId });
            }

            ViewBag.BranchId = branchId;
            ViewBag.OriginalName = ingredientName;
            ViewBag.BaseUnits = Enum.GetValues(typeof(BaseUnit));
            return View(ing);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(
            int branchId,
            string ingredientName,
            string name,
            decimal quantity,
            decimal unitCost,
            string? displayUnit)
        {
            if (!TryGetUserId(out var userId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _ingredientService.UpdateAsync(userId, ingredientName, name, quantity, unitCost, displayUnit);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index", new { branchId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int branchId, string ingredientName)
        {
            if (!TryGetUserId(out var userId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _ingredientService.DeleteAsync(userId, ingredientName);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index", new { branchId });
        }
    }
}


