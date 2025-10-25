using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IQrService
    {
        Task<QrResolveResult> ResolveTableAsync(string qrToken);
    }

    public class QrResolveResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public CafeTable? Table { get; set; }
        public Branch? Branch { get; set; }
        public IEnumerable<MenuItem>? MenuItems { get; set; }

        public static QrResolveResult Success(CafeTable table, Branch branch, IEnumerable<MenuItem> menuItems)
            => new QrResolveResult { IsSuccess = true, Table = table, Branch = branch, MenuItems = menuItems };

        public static QrResolveResult Failed(string message)
            => new QrResolveResult { IsSuccess = false, Message = message };
    }
}
