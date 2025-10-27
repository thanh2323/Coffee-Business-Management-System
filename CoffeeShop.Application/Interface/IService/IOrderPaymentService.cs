using CoffeeShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IOrderPaymentService
    {
        public Task<bool> ConvertTempOrderToRealOrderAsync(string tempOrderId, PaymentGateway gateway);
    }
}
