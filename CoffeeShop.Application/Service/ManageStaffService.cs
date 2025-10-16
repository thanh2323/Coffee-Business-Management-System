using System.Security.Claims;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CoffeeShop.Application.Service
{
    public class ManageStaffService : IManageStaffService
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuthService _authService;

        public ManageStaffService(IUnitOfWork uow, IAuthService authService)
        {
            _uow = uow;
            _authService = authService;
        }


        public async Task<StaffResult> CreateStaffAsync(string username, string email, string password, StaffRole position, int branchId)
        {

            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return StaffResult.Failed("User not found");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return StaffResult.Failed("Branch not found");

            var userCurrent = _authService.CanManageBranch(user, branch);

            var existingUserByUsername = await _uow.Users.GetByUsernameAsync(username);
            if (existingUserByUsername != null)
                return StaffResult.Failed("Username already exists");
            var existingUserByEmail = await _uow.Users.GetByEmailAsync(email);
            if (existingUserByEmail != null)

                return StaffResult.Failed("Email already exists");


            var passwordHasher = new PasswordHasher<User>();
            var staffUser = new User
            {
                Username = username.Trim(),
                Email = email.Trim(),
                PasswordHash = passwordHasher.HashPassword(null!, password),
                Role = UserRole.Staff,
                BusinessId = user.BusinessId,
                BranchId = branchId,
                CreatedAt = DateTime.UtcNow,
                StaffProfile = new StaffProfile
                {
                    Position = position,
                    CreatedAt = DateTime.UtcNow
                }
            };
            _uow.Users.Add(staffUser);
            await _uow.SaveChangesAsync();


            return StaffResult.Success(staffUser, staffUser.StaffProfile, "Staff account created");
        }

        public async Task<IEnumerable<User>> GetStaffByBranchAsync(int? branchId)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                throw new Exception("User not found");

            int targetBranchId;
            if (branchId.HasValue)
                if (branchId.Value <= 0)
                    throw new ArgumentException("Invalid branch ID.");
                else
                    targetBranchId = branchId.Value;
            else if (user.BranchId.HasValue)
                targetBranchId = user.BranchId.Value;
            else
                throw new Exception("No branch specified");
            return await _uow.Users.GetStaffByBranchAsync(targetBranchId);

        }
    }
}


