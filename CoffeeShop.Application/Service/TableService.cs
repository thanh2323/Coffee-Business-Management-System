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

        public TableService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<CafeTable>> GetByBranchAsync(int userId, int branchId)
        {
            var owner = await _uow.Users.GetByIdAsync(userId);
            if (owner == null)
                return Enumerable.Empty<CafeTable>();

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return Enumerable.Empty<CafeTable>();

            var isOwnerManaging = owner.BusinessId.HasValue && branch.BusinessId == owner.BusinessId.Value && owner.Role == UserRole.Owner;
            var isManagerManaging = owner.Role == UserRole.Staff && owner.BranchId.HasValue && owner.BranchId.Value == branchId && owner.StaffProfile?.Position == StaffRole.Manager;
            if (!isOwnerManaging && !isManagerManaging)
                return Enumerable.Empty<CafeTable>();

            return await _uow.CafeTables.GetByBranchAsync(branchId);
        }

        public async Task<TableResult> CreateAsync(int userId, int branchId, int tableNumber)
        {
            if (tableNumber <= 0)
                return TableResult.Failed("Invalid table number");

            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null)
                return TableResult.Failed("User not found");

            var branch = await _uow.Branches.GetByIdAsync(branchId);
            if (branch == null)
                return TableResult.Failed("Branch not found");

            // Authorization: Owner of business or Manager of this branch
            var isOwnerManaging = user.Role == UserRole.Owner
                        && user.BusinessId.HasValue
                        && branch.BusinessId == user.BusinessId.Value;

            var isManagerManaging = user.Role == UserRole.Staff
                        && user.BranchId.HasValue
                        && user.BranchId.Value == branchId
                        && user.StaffProfile?.Position == StaffRole.Manager;
            if (!isOwnerManaging && !isManagerManaging)
                return TableResult.Failed("Not authorized to manage this branch");

            // Business active check only for owner path
            if (isOwnerManaging)
            {
                var business = await _uow.Businesses.GetByIdAsync(branch.BusinessId);
                if (business == null)
                    return TableResult.Failed("Business not found");
                if (!business.IsActive)
                    return TableResult.Failed("Business is not active");
            }

            var exists = await _uow.CafeTables.GetByBranchAndNumberAsync(branchId, tableNumber);
            if (exists != null)
                return TableResult.Failed("Table number already exists in this branch");

            var table = new CafeTable
            {
                BranchId = branchId,
                TableNumber = tableNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _uow.CafeTables.Add(table);
            await _uow.SaveChangesAsync();

            return TableResult.Success(table, "Table created");
        }

        public async Task<TableResult> GenerateQrAsync(int userId, int tableId, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                return TableResult.Failed("BaseUrl required");

            var owner = await _uow.Users.GetByIdAsync(userId);
            if (owner == null)
                return TableResult.Failed("User not found");

            // Load table and verify ownership via branch
            var table = await _uow.CafeTables.GetByIdAsync(tableId);
            if (table == null)
                return TableResult.Failed("Table not found");
            var branch = await _uow.Branches.GetByIdAsync(table.BranchId);
            if (branch == null)
                return TableResult.Failed("Branch not found");

            // Authorization: Owner of business or Manager of this branch
            var isOwnerManaging = owner.BusinessId.HasValue
                && branch.BusinessId == owner.BusinessId.Value
                && owner.Role == UserRole.Owner;

            var isManagerManaging = owner.Role == UserRole.Staff
                && owner.BranchId.HasValue
                && owner.BranchId.Value == branch.BranchId
                && owner.StaffProfile?.Position == StaffRole.Manager;
            if (!isOwnerManaging && !isManagerManaging)
                return TableResult.Failed("Not authorized to manage this branch");

            // Simple QR link template; tokenization can be added later
            if (string.IsNullOrEmpty(table.QRCode))
            {
                table.QRCode = Guid.NewGuid().ToString();
                table.MarkAsUpdated();
                _uow.CafeTables.Update(table);
                await _uow.SaveChangesAsync();
            }
            var qrUrl = $"{baseUrl.TrimEnd('/')}/qr/resolve?t={table.QRCode}";

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(qrUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrPng = new PngByteQRCode(qrData);
            byte[] qrBytes = qrPng.GetGraphic(20); 

            string qrBase64 = Convert.ToBase64String(qrBytes);

            return TableResult.Success(table, qrBase64, qrUrl, "QR generated");
        }

        public async Task<TableResult> DeleteAsync(int userId, int tableId)
        {
            var owner = await _uow.Users.GetByIdAsync(userId);
            if (owner == null)
                return TableResult.Failed("User not found");

            // Load table and verify ownership via branch
            var table = await _uow.CafeTables.GetByIdAsync(tableId);
            if (table == null)
                return TableResult.Failed("Table not found");
            var branch = await _uow.Branches.GetByIdAsync(table.BranchId);
            if (branch == null)
                return TableResult.Failed("Branch not found");

            // Authorization: Owner of business or Manager of this branch
            var isOwnerManaging = owner.BusinessId.HasValue
                && branch.BusinessId == owner.BusinessId.Value
                && owner.Role == UserRole.Owner;

            var isManagerManaging = owner.Role == UserRole.Staff
                && owner.BranchId.HasValue
                && owner.BranchId.Value == branch.BranchId
                && owner.StaffProfile?.Position == StaffRole.Manager;
            if (!isOwnerManaging && !isManagerManaging)
                return TableResult.Failed("Not authorized to manage this branch");

            _uow.CafeTables.SoftDelete(table);
            await _uow.SaveChangesAsync();

            return TableResult.Success(table, "Table deleted successfully");
        }
    }
}


