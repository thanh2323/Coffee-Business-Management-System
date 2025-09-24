using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Service
{
    public class BranchService : IBranchService
    {
        private readonly IUnitOfWork _uow;

        public BranchService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<Branch>> GetBranchesForOwnerAsync(int ownerUserId)
        {
            var owner = await _uow.Users.GetByIdAsync(ownerUserId);
            if (owner == null || !owner.BusinessId.HasValue)
                return Enumerable.Empty<Branch>();
            return await _uow.Branches.GetByBusinessIdAsync(owner.BusinessId.Value);
        }

        public async Task<BranchResult> CreateBranchAsync(int ownerUserId, string name, string? address, TimeSpan openTime, TimeSpan closeTime)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BranchResult.Failed("Branch name is required");
            if (openTime >= closeTime)
                return BranchResult.Failed("Open time must be earlier than close time");

            var owner = await _uow.Users.GetByIdAsync(ownerUserId);
            if (owner == null || !owner.BusinessId.HasValue)
                return BranchResult.Failed("Owner has no business");
            // Ensure business is active before allowing branch creation
            var business = await _uow.Businesses.GetByIdAsync(owner.BusinessId.Value);
            if (business == null)
                return BranchResult.Failed("Business not found");
            if (!business.IsActive)
                return BranchResult.Failed("Business is not active. Please complete payment to activate.");
            if (await _uow.Branches.ExistsByNameAsync(owner.BusinessId.Value, name.Trim()))
                return BranchResult.Failed("Branch name already exists");
         
            var branch = new Branch
            {
                Name = name.Trim(),
                Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim(),
                OpenTime = openTime,
                CloseTime = closeTime,
                BusinessId = owner.BusinessId.Value,
                CreatedAt = DateTime.UtcNow
            };

            _uow.Branches.Add(branch);
            await _uow.SaveChangesAsync();
            return BranchResult.Success(branch, "Branch created");
        }

        public async Task<BranchResult> UpdateBranchAsync(int ownerUserId, int branchId, string name, string? address, TimeSpan openTime, TimeSpan closeTime)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BranchResult.Failed("Branch name is required");
            if (openTime >= closeTime)
                return BranchResult.Failed("Open time must be earlier than close time");

            var owner = await _uow.Users.GetByIdAsync(ownerUserId);
            if (owner == null || !owner.BusinessId.HasValue)
                return BranchResult.Failed("Owner has no business");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null || branch.BusinessId != owner.BusinessId.Value)
                return BranchResult.Failed("Branch not found");

            branch.Name = name.Trim();
            branch.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
            branch.OpenTime = openTime;
            branch.CloseTime = closeTime;
            branch.MarkAsUpdated();

            _uow.Branches.Update(branch);
            await _uow.SaveChangesAsync();
            return BranchResult.Success(branch, "Branch updated");
        }

        public async Task<(string businessName, string ownerName)> GetOwnerContextAsync(int ownerUserId)
        {
            var owner = await _uow.Users.GetByIdAsync(ownerUserId);
            if (owner == null)
                return (string.Empty, string.Empty);
            var businessName = string.Empty;
            if (owner.BusinessId.HasValue)
            {
                var business = await _uow.Businesses.GetByIdAsync(owner.BusinessId.Value);
                businessName = business?.Name ?? string.Empty;
            }
            return (businessName, owner.Username);
        }

        public async Task<BranchResult> DeleteBranchAsync(int ownerUserId, int branchId)
        {
            var owner = await _uow.Users.GetByIdAsync(ownerUserId);
            if (owner == null || !owner.BusinessId.HasValue)
                return BranchResult.Failed("Owner has no business");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null || branch.BusinessId != owner.BusinessId.Value)
                return BranchResult.Failed("Branch not found");

            _uow.Branches.SoftDelete(branch);
            await _uow.SaveChangesAsync();
            return BranchResult.Success(branch, "Branch deleted");
        }
    }
}


