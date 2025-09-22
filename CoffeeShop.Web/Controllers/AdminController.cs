using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoffeeShop.Application.Interface.IService;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var all = await _adminService.GetAllBusinessesAsync();
            return View(all);
        }

        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await _adminService.ActivateBusinessAsync(id);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _adminService.DeactivateBusinessAsync(id);
            TempData[result.IsSuccess ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index");
        }
    }
}


