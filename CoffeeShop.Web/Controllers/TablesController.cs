
﻿using System.Security.Claims;
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

        public TablesController(ITableService tableService, IBranchService branchService, IUnitOfWork uow, IQrService qrService)
        {
            _tableService = tableService;
            _branchService = branchService;
          
            _uow = uow;
            _qrService = qrService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int branchId)
        {
          
            var tables = await _tableService.GetByBranchAsync(branchId);
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
          
            var result = await _tableService.CreateAsync( branchId, tableNumber);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index", new { branchId });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateQr(int branchId, int tableId)
        {
      

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _qrService.GenerateQrAsync(tableId, baseUrl);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index", new { branchId });
        }

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

            var result = await _tableService.DeleteAsync(tableId);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;

            // Redirect về danh sách table của branch
            return RedirectToAction("Index", new { branchId });

        }
    }
}

