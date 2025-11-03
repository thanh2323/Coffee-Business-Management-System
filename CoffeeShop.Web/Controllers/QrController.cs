using CoffeeShop.Application.Interface.IService;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers
{
    public class QrController : Controller
    {
        private readonly IQrService _qrService;

        public QrController(IQrService qrService)
        {
            _qrService = qrService;
        }

        [HttpGet]
        public async Task<IActionResult> Resolve(string t)
        {
            var result = await _qrService.ResolveTableAsync(t);
            
            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "GuestOrder", new { 
                tableId = result.Table!.TableId, 
                branchId = result.Table.BranchId 
            });
        }
    }
}
