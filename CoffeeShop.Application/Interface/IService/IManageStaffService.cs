using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IManageStaffService
    {
     
        Task<IEnumerable<User>> GetStaffByBranchAsync(int? branchId);
        Task<StaffResult> CreateStaffAsync(string username, string email, string password, StaffRole position, int branchId);
    }

    public class StaffResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
        public StaffProfile? StaffProfile { get; set; }

        public static StaffResult Success(User user, StaffProfile staffProfile, string message = "Staff created")
            => new StaffResult { IsSuccess = true, User = user, StaffProfile = staffProfile, Message = message };

        public static StaffResult Failed(string message)
            => new StaffResult { IsSuccess = false, Message = message };
    }
}


