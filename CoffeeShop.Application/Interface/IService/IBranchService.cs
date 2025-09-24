using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IBranchService
    {
        Task<IEnumerable<Branch>> GetBranchesForOwnerAsync(int ownerUserId);
        Task<BranchResult> CreateBranchAsync(int ownerUserId, string name, string? address, TimeSpan openTime, TimeSpan closeTime);
        Task<BranchResult> UpdateBranchAsync(int ownerUserId, int branchId, string name, string? address, TimeSpan openTime, TimeSpan closeTime);
        Task<(string businessName, string ownerName)> GetOwnerContextAsync(int ownerUserId);
        Task<BranchResult> DeleteBranchAsync(int ownerUserId, int branchId);
    }

    public class BranchResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Branch? Branch { get; set; }

        public static BranchResult Success(Branch branch, string message = "Success") => new BranchResult { IsSuccess = true, Branch = branch, Message = message };
        public static BranchResult Failed(string message) => new BranchResult { IsSuccess = false, Message = message };
    }
}


