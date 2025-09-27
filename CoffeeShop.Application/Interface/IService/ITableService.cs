using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IService
{
    public interface ITableService
    {
        Task<IEnumerable<CafeTable>> GetByBranchAsync(int userId, int branchId);
        Task<TableResult> CreateAsync(int userId, int branchId, int tableNumber);
        Task<TableResult> GenerateQrAsync(int userId, int tableId, string baseUrl);
        Task<TableResult> DeleteAsync(int userId, int tableId);
    }

    public class TableResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public CafeTable? Table { get; set; }
        public string? QRCodeBase64 { get; set; }
        public string? QRCodeUrl { get; set; }

        public static TableResult Success(CafeTable table, string message = "Success")
            => new TableResult { IsSuccess = true, Table = table, Message = message };
        public static TableResult Success(CafeTable table, string qrCodeBase64, string qrCodeUrl, string message = "Success") 
            => new TableResult { IsSuccess = true, Table = table, QRCodeBase64 = qrCodeBase64, QRCodeUrl = qrCodeUrl, Message = message };
        public static TableResult Failed(string message) => new TableResult { IsSuccess = false, Message = message };
    }
}



