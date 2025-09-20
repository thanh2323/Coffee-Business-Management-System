using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IRepo
{
    public interface ICustomerRepository : IBaseRepository<Customer>
    {
        Task<Customer?> GetByPhoneAsync(string phone);
        Task<IEnumerable<Customer>> GetByBranchIdAsync(int branchId);
    }
}