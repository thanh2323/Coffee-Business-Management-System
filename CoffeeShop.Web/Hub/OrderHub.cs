using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace CoffeeShop.Web.Hubs
{
    public class OrderHub : Hub
    {
        public async Task JoinBranchGroup(int branchId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"branch-{branchId}");
        }

        public async Task JoinOrderGroup(int orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
        }
    }
}
