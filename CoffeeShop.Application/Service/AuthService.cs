using System.Security.Claims;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Domain.Rules;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace CoffeeShop.Application.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByEmailAsync(email);
                if (user == null)
                    return AuthResult.Failed("User not found");

                var passwordHasher = new PasswordHasher<User>();
                var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
                if (result != PasswordVerificationResult.Success)
                    return AuthResult.Failed("Invalid password");

                // Create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                };
                //Owner have BusinessId, 
                if (user.BusinessId.HasValue)
                    claims.Add(new Claim("BusinessId", user.BusinessId.Value.ToString()));
                //Staff have BusinessId and BranchId
                if (user.BranchId.HasValue)
                    claims.Add(new Claim("BranchId", user.BranchId.Value.ToString()));
                // Staff have Position
                if (user.StaffProfile != null)
                    claims.Add(new Claim("Position", user.StaffProfile.Position.ToString()));

                // Create identity
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Sign in
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await _httpContextAccessor.HttpContext!.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    authProperties);

                return AuthResult.Success(user);
            }
            catch (Exception ex)
            {
                return AuthResult.Failed($"Login failed: {ex.Message}");
            }
        }

        // Register a new business owner (only create User, not Business)
        public async Task<AuthResult> RegisterOwnerAsync(string username, string email, string password)
        {
            try
            {
                if (!UserRules.IsValidUserData(username, email, password))
                    return AuthResult.Failed("Invalid user data");

                var existingUser = await _unitOfWork.Users.GetByUsernameAsync(username);
                if (existingUser != null)
                    return AuthResult.Failed("Username already exists");
                var existingEmail = await _unitOfWork.Users.GetByEmailAsync(email);
                if (existingEmail != null)
                    return AuthResult.Failed("Email already exists");

                // Hash password
                var passwordHasher = new PasswordHasher<User>();

                var user = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = passwordHasher.HashPassword(null!, password),
                    Role = UserRole.Owner,
                    BusinessId = null // Chưa có business, sẽ tạo sau
                };

                _unitOfWork.Users.Add(user);
                await _unitOfWork.SaveChangesAsync();

                return AuthResult.Success(user);
            }
            catch (Exception ex)
            {
                return AuthResult.Failed($"Registration failed: {ex.Message}");
            }
        }
        // Logout the current user
        public async Task LogoutAsync()
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }


        public async Task<User?> GetCurrentUserAsync()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return null;

            return await _unitOfWork.Users.GetByIdAsync(userId);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new ArgumentException("User not found");

                var passwordHasher = new PasswordHasher<User>();
                var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword);
                if (result != PasswordVerificationResult.Success)
                    return false;

                if (!UserRules.IsValidPassword(newPassword))
                    throw new ArgumentException(UserRules.GetPasswordRequirements());
                user.PasswordHash = passwordHasher.HashPassword(user, newPassword);

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CanManageBranch(User user, Branch branch)
        {
            if (user == null || branch == null)
                return false;

            switch (user.Role)
            {
                case UserRole.Owner:
                   
                    return user.BusinessId == branch.BusinessId;

                case UserRole.Staff:
                   
                    return user.BranchId == branch.BranchId
                        && user.StaffProfile?.Position == StaffRole.Manager;

                default:
                    return false;
            }
        }

    }
}
