using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IService
{
    public interface ITableService
    {
        Task<IEnumerable<CafeTable>> GetByBranchAsync(int userId, int branchId);
        Task<TableResult> CreateAsync(int userId, int branchId, int tableNumber);
        Task<TableResult> GenerateQrAsync(int userId, int tableId, string baseUrl);
    }

    public class TableResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public CafeTable? Table { get; set; }

        public static TableResult Success(CafeTable table, string message = "Success") => new TableResult { IsSuccess = true, Table = table, Message = message };
        public static TableResult Failed(string message) => new TableResult { IsSuccess = false, Message = message };
    }
}



