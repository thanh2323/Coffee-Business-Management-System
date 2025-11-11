using CoffeeShop.Application.Interface;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace CoffeeShop.Application.Service
{
    public class TableService : ITableService
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuthService _authService;
        private readonly IBranchResolverService _branchResolver;
        public TableService(IUnitOfWork uow, IAuthService authService, IBranchResolverService branchResolver)
        {
            _branchResolver = branchResolver;
            _uow = uow;
            _authService = authService;
        }

        public async Task<IEnumerable<CafeTable>> GetByBranchAsync(int? branchId)
        {
            var targetBranchId = await _branchResolver.ResolveBranchIdAsync(branchId);

            return await _uow.CafeTables.GetByBranchAsync(targetBranchId.Value);
        }

        public async Task<TableResult> CreateAsync(int branchId, int tableNumber)
        {
   
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return TableResult.Failed("User not found");

            var targetBranchId = await _branchResolver.ResolveBranchIdAsync(branchId);

            var branch = await _uow.Branches.GetByIdAsync(targetBranchId);
            if (branch == null)
                return TableResult.Failed("Branch not found");

            var userCanManage = _authService.CanManageBranch(user, branch);
            if (!userCanManage)
                return TableResult.Failed("Not authorized to access");

            if (tableNumber <= 0)
                return TableResult.Failed("Invalid table number");

            var exists = await _uow.CafeTables.GetByBranchAndNumberAsync(branchId, tableNumber);
            if (exists != null)
                return TableResult.Failed("Table number already exists in this branch");

            var table = new CafeTable
            {
                BranchId = branch.BranchId,
                TableNumber = tableNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _uow.CafeTables.Add(table);
            await _uow.SaveChangesAsync();

            return TableResult.Success(table, "Table created");
        }

       
        public async Task<TableResult> DeleteAsync( int tableId)
        {
            var table = await _uow.CafeTables.GetByIdAsync(tableId);
            if (table == null)
                return TableResult.Failed("Table not found");
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
                return TableResult.Failed("User not found");

            var branch = await _uow.Branches.GetByIdAsync(table.BranchId);
            if (branch == null)
                return TableResult.Failed("Branch not found");

            var userCanManage = _authService.CanManageBranch(user, branch);
            if (!userCanManage)
                return TableResult.Failed("Not authorized to access");

            _uow.CafeTables.SoftDelete(table);
            await _uow.SaveChangesAsync();

            return TableResult.Success(table, "Table deleted successfully");
        }
    }
}


