using System.Security.Claims;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Service;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

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

   
        [HttpGet]
        public async Task<IActionResult> Index(int branchId)
        {
            var ingredients = await _ingredientService.GetByBranchAsync(branchId);

            if (ingredients == null || !ingredients.Any())
            {
                TempData["Error"] = "This branch has no ingredients yet or does not exist.";
               
            }

            ViewBag.BranchId = branchId;
            return View(ingredients);
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
          
            var result = await _ingredientService.CreateAsync(branchId, name, quantity, unitCost, displayUnit);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index", new { branchId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int branchId, string ingredientName)
        {
          
            var list = await _ingredientService.GetByBranchAsync(branchId);
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
         
            var result = await _ingredientService.UpdateAsync(branchId, ingredientName, name, quantity, unitCost, displayUnit);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index", new { branchId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int branchId, string ingredientName)
        {
                     var result = await _ingredientService.DeleteAsync( ingredientName);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index", new { branchId });
        }
    }
}


