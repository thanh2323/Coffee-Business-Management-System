using System.Security.Claims;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Policy = "RequireOwnerOrManager")]
    public class TablesController : Controller
    {
        private readonly ITableService _tableService;
        private readonly IBranchService _branchService;
        private readonly IAuthService _authService;
        private readonly IUnitOfWork _uow;

        public TablesController(ITableService tableService, IBranchService branchService, IAuthService authService, IUnitOfWork uow)
        {
            _tableService = tableService;
            _branchService = branchService;
            _authService = authService;
            _uow = uow;
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

            var tables = await _tableService.GetByBranchAsync(userId, branchId);
            ViewBag.BranchId = branchId;
            return View(tables);
        }

        [HttpGet]
        public IActionResult Create(int branchId)
        {
            ViewBag.BranchId = branchId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int branchId, int tableNumber)
        {
            if (!TryGetUserId(out var userId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _tableService.CreateAsync(userId, branchId, tableNumber);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index", new { branchId });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateQr(int branchId, int tableId)
        {
            if (!TryGetUserId(out var userId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("Index", "Home");
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _tableService.GenerateQrAsync(userId, tableId, baseUrl);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index", new { branchId });
        }

        [HttpGet]
        public async Task<IActionResult> QrCode(int tableId)
        {
            if (!TryGetUserId(out var userId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("Index", "Home");
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _tableService.GenerateQrAsync(userId, tableId, baseUrl);
            
            if (result.IsSuccess && !string.IsNullOrEmpty(result.QRCodeBase64))
            {
                var qrBytes = Convert.FromBase64String(result.QRCodeBase64);
                return File(qrBytes, "image/png");
            }
            
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int tableId)
        {
            if (!TryGetUserId(out var userId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _tableService.DeleteAsync(userId, tableId);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            
            // Redirect back to the branch's table list
            var table = await _uow.CafeTables.GetByIdAsync(tableId);
            return RedirectToAction("Index", new { branchId = table?.BranchId });
        }
    }
}


