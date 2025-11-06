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
        private readonly IQrService _qrService;
        private readonly IBranchService _branchService;
        private readonly IUnitOfWork _uow;

        public TablesController(
            ITableService tableService,
            IBranchService branchService,
            IUnitOfWork uow,
            IQrService qrService)
        {
            _tableService = tableService;
            _branchService = branchService;
            _uow = uow;
            _qrService = qrService;
        }

        // ✅ Xem danh sách bàn của chi nhánh (theo URL hoặc Claims)
        [HttpGet]
        public async Task<IActionResult> Index(int? branchId)
        {
            int resolvedBranchId = await ResolveBranchId(branchId);

            var tables = await _tableService.GetByBranchAsync(resolvedBranchId);
            ViewBag.BranchId = resolvedBranchId;
            return View(tables);
        }

        // ✅ Form tạo bàn
        [HttpGet]
        public async Task<IActionResult> Create(int? branchId)
        {
            int resolvedBranchId = await ResolveBranchId(branchId);
            ViewBag.BranchId = resolvedBranchId;
            return View();
        }

        // ✅ Tạo bàn — chỉ cho phép Manager tạo trong chi nhánh của họ
        [HttpPost]
        public async Task<IActionResult> Create(int? branchId, int tableNumber)
        {
            int resolvedBranchId = await ResolveBranchId(branchId);

            // 🔒 Kiểm tra quyền Manager
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userBranchClaim = User.FindFirst("BranchId")?.Value;

            if (userRole == "Staff" && userBranchClaim != null)
            {
                if (int.Parse(userBranchClaim) != resolvedBranchId)
                {
                    TempData["Error"] = "You are not allowed to create tables for another branch.";
                    return RedirectToAction("Index", new { branchId = int.Parse(userBranchClaim) });
                }
            }

            var result = await _tableService.CreateAsync(resolvedBranchId, tableNumber);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;

            return RedirectToAction("Index", new { branchId = resolvedBranchId });
        }

        // ✅ Tạo mã QR cho bàn
        [HttpPost]
        public async Task<IActionResult> GenerateQr(int? branchId, int tableId)
        {
            int resolvedBranchId = await ResolveBranchId(branchId);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _qrService.GenerateQrAsync(tableId, baseUrl);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;

            return RedirectToAction("Index", new { branchId = resolvedBranchId });
        }

        // ✅ Hiển thị hình QR
        [HttpGet]
        public async Task<IActionResult> QrCode(int tableId)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _qrService.GenerateQrAsync(tableId, baseUrl);

            if (result.IsSuccess && !string.IsNullOrEmpty(result.QRCodeBase64))
            {
                var qrBytes = Convert.FromBase64String(result.QRCodeBase64);
                return File(qrBytes, "image/png");
            }

            return NotFound();
        }

        // ✅ Xoá bàn — Manager chỉ được xoá trong chi nhánh của họ
        [HttpPost]
        public async Task<IActionResult> Delete(int tableId)
        {
            var table = await _uow.CafeTables.GetByIdAsync(tableId);
            if (table == null)
            {
                TempData["Error"] = "Table not found.";
                return RedirectToAction("Index", "Home");
            }

            var branchId = table.BranchId;

            // 🔒 Kiểm tra quyền Manager
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userBranchClaim = User.FindFirst("BranchId")?.Value;

            if (userRole == "Staff" && userBranchClaim != null)
            {
                if (int.Parse(userBranchClaim) != branchId)
                {
                    TempData["Error"] = "You are not allowed to delete tables from another branch.";
                    return RedirectToAction("Index", new { branchId = int.Parse(userBranchClaim) });
                }
            }

            var result = await _tableService.DeleteAsync(tableId);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;

            return RedirectToAction("Index", new { branchId });
        }

        // 🧩 Helper: Tự động lấy branchId từ URL hoặc Claims
        private async Task<int> ResolveBranchId(int? branchId)
        {
            if (branchId.HasValue && branchId.Value > 0)
                return branchId.Value;

            var branchClaim = User.FindFirst("BranchId")?.Value;
            if (!string.IsNullOrEmpty(branchClaim))
                return int.Parse(branchClaim);

            // Nếu Owner mà chưa có branchId — lấy branch đầu tiên của business
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Owner")
            {
                var businessIdClaim = User.FindFirst("BusinessId")?.Value;
                if (businessIdClaim != null)
                {
                    var firstBranch = (await _branchService.GetByBusinessAsync(int.Parse(businessIdClaim)))?.FirstOrDefault();
                    if (firstBranch != null) return firstBranch.BranchId;
                }
            }

            throw new Exception("No branchId found in URL or user claims.");
        }
    }
}
