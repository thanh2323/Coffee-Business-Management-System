using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IQrService
    {
        Task<QrResult> ResolveTableAsync(string qrToken);
        Task<QrResult> GenerateQrAsync(int tableId, string baseUrl);
    }

    public class QrResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public CafeTable? Table { get; set; }
        public Branch? Branch { get; set; }
        public IEnumerable<MenuItem>? MenuItems { get; set; }
        public string? QRCodeBase64 { get; set; }
        public string? QRCodeUrl { get; set; }


        public static QrResult Success(CafeTable table, Branch branch, IEnumerable<MenuItem> menuItems)
            => new QrResult { IsSuccess = true, Table = table, Branch = branch, MenuItems = menuItems };
        public static QrResult Success(CafeTable table, string qrCodeBase64, string qrCodeUrl, string message = "Success")
          => new QrResult { IsSuccess = true, Table = table, QRCodeBase64 = qrCodeBase64, QRCodeUrl = qrCodeUrl, Message = message };
        public static QrResult Failed(string message)
            => new QrResult { IsSuccess = false, Message = message };
    }
}
