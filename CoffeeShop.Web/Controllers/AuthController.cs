using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("RedirectByRole", "Auth");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Email and password are required.";
                return View();
            }

            var result = await _authService.LoginAsync(email, password);

            if (result.IsSuccess)
            {
                TempData["Success"] = "Login successful!";
                return RedirectToAction("RedirectByRole", "Auth");
            }



            TempData["Error"] = result.Message;
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult RedirectByRole()
        {
            // 🔹 Lấy thông tin Role và Position từ cookie (claims)
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var position = User.FindFirst("Position")?.Value;

            // 🔹 Điều hướng đến trang phù hợp
            switch (role)
            {
                case "Admin":
                    return RedirectToAction("Index", "Admin");

                case "Owner":
                    return RedirectToAction("My", "Business");

                case "Staff":
                    if (position == "Manager")
                        return RedirectToAction("Index", "Manager");
                    else
                        return RedirectToAction("Index", "Order");

                default:
                    return RedirectToAction("Index", "Home");
            }
        }


        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "All fields are required.";
                return View();
            }

            if (password != confirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return View();
            }

            var result = await _authService.RegisterOwnerAsync(username, email, password);

            if (result.IsSuccess)
            {
                TempData["Success"] = "Registration successful! You can now login.";
                return RedirectToAction("Login");
            }

            TempData["Error"] = result.Message;
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            TempData["Success"] = "You have been logged out.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpGet]
        public IActionResult Forbidden()
        {
            return View();
        }
    }
}
