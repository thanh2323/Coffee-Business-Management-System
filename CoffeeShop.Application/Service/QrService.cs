using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using QRCoder;

namespace CoffeeShop.Application.Service
{
    public class QrService : IQrService
    {
        private readonly IUnitOfWork _uow;

        public QrService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<QrResult> GenerateQrAsync(int tableId, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                return QrResult.Failed("BaseUrl required");

          
            // Load table and verify ownership via branch
            var table = await _uow.CafeTables.GetByIdAsync(tableId);
            if (table == null)
                return QrResult.Failed("Table not found");
            var branch = await _uow.Branches.GetByIdAsync(table.BranchId);
            if (branch == null)
                return QrResult.Failed("Branch not found");

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

            return QrResult.Success(table, qrBase64, qrUrl, "QR generated");
        }

        public async Task<QrResult> ResolveTableAsync(string qrToken)
        {
            if (string.IsNullOrWhiteSpace(qrToken))
                return QrResult.Failed("Invalid QR code");

            // Find table by QR token
            var table = await _uow.CafeTables.GetByQrTokenAsync(qrToken);
            if (table == null)
                return QrResult.Failed("Table not found or QR code expired");
                
            // Get branch info
            var branch = await _uow.Branches.GetByIdAsync(table.BranchId);
            if (branch == null)
                return QrResult.Failed("Branch not found");

            // Get available menu items for this branch
            var menuItems = await _uow.MenuItems.GetByBranchIdAsync(table.BranchId);

            return QrResult.Success(table, branch, menuItems);
        }
    }
}
