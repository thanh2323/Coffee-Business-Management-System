using System.Security.Claims;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Policy = "RequireOwnerOrManager")]
    public class IngredientsController : Controller
    {
        private readonly IIngredientService _ingredientService;
        private readonly IAuthService _authService;

        public IngredientsController(IIngredientService ingredientService, IAuthService authService)
        {
            _ingredientService = ingredientService;
            _authService = authService;
        }

        // =========================
        // INDEX
        // =========================
        [HttpGet]
        public async Task<IActionResult> Index(int? branchId)
        {
            var me = await _authService.GetCurrentUserAsync();
            if (me == null) return RedirectToAction("Login", "Auth");

            int resolvedBranchId;

            // ✅ OWNER: dùng branchId truyền vào
            if (me.Role == CoffeeShop.Domain.Enums.UserRole.Owner)
            {
                if (!branchId.HasValue || branchId.Value <= 0)
                {
                    TempData["ErrorMessage"] = "Please select a branch to view ingredients.";
                    return RedirectToAction("Index", "Branch");
                }
                resolvedBranchId = branchId.Value;
            }
            else
            {
                // ✅ MANAGER: chỉ được xem nguyên liệu trong chi nhánh của mình
                if (!me.BranchId.HasValue)
                {
                    TempData["ErrorMessage"] = "Your account is not assigned to any branch.";
                    return RedirectToAction("Forbidden", "Auth");
                }
                resolvedBranchId = me.BranchId.Value;

                // Nếu cố truy cập branchId khác thì chuyển về chi nhánh của manager
                if (branchId.HasValue && branchId.Value != resolvedBranchId)
                    return RedirectToAction(nameof(Index), new { branchId = resolvedBranchId });
            }

            var ingredients = await _ingredientService.GetByBranchAsync(resolvedBranchId);
            if (ingredients == null || !ingredients.Any())
            {
                TempData["InfoMessage"] = "This branch has no ingredients yet.";
            }

            ViewBag.BranchId = resolvedBranchId;
            return View(ingredients);
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
            if (me.Role == CoffeeShop.Domain.Enums.UserRole.Owner)
            {
                if (!branchId.HasValue || branchId.Value <= 0)
                {
                    TempData["ErrorMessage"] = "Please select a branch to add ingredients.";
                    return RedirectToAction("Index", "Branch");
                }
                resolvedBranchId = branchId.Value;
            }
            else
            {
                // Manager luôn dùng branchId từ claim
                if (!me.BranchId.HasValue) return Forbid();
                resolvedBranchId = me.BranchId.Value;
            }

            ViewBag.BranchId = resolvedBranchId;
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
            var me = await _authService.GetCurrentUserAsync();
            if (me == null) return RedirectToAction("Login", "Auth");

            // Manager: ép branchId về chi nhánh của họ
            if (me.Role != CoffeeShop.Domain.Enums.UserRole.Owner && me.BranchId.HasValue)
                branchId = me.BranchId.Value;

            var result = await _ingredientService.CreateAsync(branchId, name, quantity, unitCost, displayUnit);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;

            return RedirectToAction(nameof(Index), new { branchId });
        }

        // =========================
        // EDIT
        // =========================
        [HttpGet]
        public async Task<IActionResult> Edit(int branchId, string ingredientName)
        {
            var me = await _authService.GetCurrentUserAsync();
            if (me == null) return RedirectToAction("Login", "Auth");

            // Manager: kiểm tra quyền
            if (me.Role != CoffeeShop.Domain.Enums.UserRole.Owner && me.BranchId.HasValue)
            {
                if (me.BranchId.Value != branchId)
                    return Forbid();
                branchId = me.BranchId.Value;
            }

            var list = await _ingredientService.GetByBranchAsync(branchId);
            var ing = list.FirstOrDefault(i => i.Name == ingredientName);
            if (ing == null)
            {
                TempData["Error"] = "Ingredient not found.";
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
            var me = await _authService.GetCurrentUserAsync();
            if (me == null) return RedirectToAction("Login", "Auth");

            if (me.Role != CoffeeShop.Domain.Enums.UserRole.Owner && me.BranchId.HasValue)
                branchId = me.BranchId.Value;

            var result = await _ingredientService.UpdateAsync(branchId, ingredientName, name, quantity, unitCost, displayUnit);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Index), new { branchId });
        }

        // =========================
        // DELETE
        // =========================
        [HttpPost]
        public async Task<IActionResult> Delete(int branchId, string ingredientName)
        {
            var me = await _authService.GetCurrentUserAsync();
            if (me == null) return RedirectToAction("Login", "Auth");

            if (me.Role != CoffeeShop.Domain.Enums.UserRole.Owner && me.BranchId.HasValue)
                branchId = me.BranchId.Value;

            var result = await _ingredientService.DeleteAsync(ingredientName);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;

            return RedirectToAction(nameof(Index), new { branchId });
        }
    }
}
