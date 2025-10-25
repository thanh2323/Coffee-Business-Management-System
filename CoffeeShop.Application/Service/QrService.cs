using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Service
{
    public class QrService : IQrService
    {
        private readonly ICafeTableRepository _tableRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IMenuItemRepository _menuItemRepository;

        public QrService(
            ICafeTableRepository tableRepository,
            IBranchRepository branchRepository,
            IMenuItemRepository menuItemRepository)
        {
            _tableRepository = tableRepository;
            _branchRepository = branchRepository;
            _menuItemRepository = menuItemRepository;
        }

        public async Task<QrResolveResult> ResolveTableAsync(string qrToken)
        {
            if (string.IsNullOrWhiteSpace(qrToken))
                return QrResolveResult.Failed("Invalid QR code");

            // Find table by QR token
            var table = await _tableRepository.GetByQrTokenAsync(qrToken);
            if (table == null)
                return QrResolveResult.Failed("Table not found or QR code expired");

            // Get branch info
            var branch = await _branchRepository.GetByIdAsync(table.BranchId);
            if (branch == null)
                return QrResolveResult.Failed("Branch not found");

            // Get available menu items for this branch
            var menuItems = await _menuItemRepository.GetByBranchIdAsync(table.BranchId);

            return QrResolveResult.Success(table, branch, menuItems);
        }
    }
}
