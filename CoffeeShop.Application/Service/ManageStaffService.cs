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

            var canManage = _authService.CanManageBranch(user, branch);
            if (!canManage)
                return StaffResult.Failed("Not authorized to manage this branch");

            var existingUsername = await _uow.Users.GetByUsernameAsync(username);
            if (existingUsername != null)
                return StaffResult.Failed("Username already exists");

            var existingEmail = await _uow.Users.GetByEmailAsync(email);
            if (existingEmail != null)
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

        public async Task<StaffResult> DeleteStaffAsync(int staffId, int branchId)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return StaffResult.Failed("User not found");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return StaffResult.Failed("Branch not found");

            if (!_authService.CanManageBranch(user, branch))
                return StaffResult.Failed("Not authorized to manage this branch");

            var staffUser = await _uow.Users.GetByIdAsync(staffId);
            if (staffUser == null || staffUser.Role != UserRole.Staff || staffUser.BranchId != branchId)
                return StaffResult.Failed("Staff member not found in the specified branch");

            _uow.Users.SoftDelete(staffUser);
            await _uow.SaveChangesAsync();
            return StaffResult.Success(staffUser, staffUser.StaffProfile, "Staff account deleted");
        }

        public async Task<StaffResult> UpdateStaffAsync(int staffId, string username, StaffRole? position, int branchId)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return StaffResult.Failed("User not found");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return StaffResult.Failed("Branch not found");

            if (!_authService.CanManageBranch(user, branch))
                return StaffResult.Failed("Not authorized to manage this branch");

            var staffUser = await _uow.Users.GetByIdAsync(staffId);
            if (staffUser == null || staffUser.Role != UserRole.Staff || staffUser.BranchId != branchId)
                return StaffResult.Failed("Staff member not found in the specified branch");

            staffUser.Username = username;
            if (position.HasValue && staffUser.StaffProfile != null)
                staffUser.StaffProfile.Position = position.Value;

            _uow.Users.Update(staffUser);
            await _uow.SaveChangesAsync();

            return StaffResult.Success(staffUser, staffUser.StaffProfile, "Staff account updated successfully");
        }

        public async Task<StaffResult> GetByIdAsync(int staffId)
        {
            var staffUser = await _uow.Users.GetByIdAsync(staffId);
            if (staffUser == null || staffUser.Role != UserRole.Staff)
                return StaffResult.Failed("Staff member not found");

            return StaffResult.Success(staffUser, staffUser.StaffProfile, "Staff member found");
        }

        public async Task<IEnumerable<User>> GetStaffByBranchAsync(int? branchId)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                throw new Exception("User not found");

            int targetBranchId;
            if (branchId.HasValue && branchId.Value > 0)
                targetBranchId = branchId.Value;
            else if (user.BranchId.HasValue)
                targetBranchId = user.BranchId.Value;
            else
                throw new Exception("No branch specified");

            return await _uow.Users.GetStaffByBranchAsync(targetBranchId);
        }

        // 🟢 Hàm mới cho Owner: lấy toàn bộ nhân viên trong Business
        public async Task<IEnumerable<User>> GetAllStaffAsync(int businessId)
        {
            return await _uow.Users.GetStaffByBusinessAsync(businessId);
        }
    }
}
