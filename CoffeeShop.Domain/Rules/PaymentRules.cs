using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;

namespace CoffeeShop.Domain.Rules
{
    public static class PaymentRules
    {
        // 1. Validate if a payment method can be used (can extend later: business hours, online/offline, etc.)
        public static bool IsPaymentMethodAvailable(PaymentMethod method, decimal amount)
        {
            if (method == PaymentMethod.Cash) return true;

            if (method == PaymentMethod.BankTransfer && amount >= 10000)
                return true;

            return false;
        }

        // 2. Validate payment amount
        public static bool IsFullPaymentValid(decimal paymentAmount, decimal orderTotal)
        {
            return paymentAmount >= orderTotal;
        }

        // 3. Check if refund is possible
        public static bool CanRefund(Order order)
        {
            return order.PaymentStatus == PaymentStatus.Completed &&
                   order.CurrentStatus != OrderStatus.Completed;
        }

        // 4. Calculate change amount (for cash payments)
        public static decimal CalculateChange(decimal paymentAmount, decimal orderTotal)
        {
            return Math.Max(0, paymentAmount - orderTotal);
        }

        // 5. Get payment methods available for amount
        public static List<PaymentMethod> GetAvailablePaymentMethods(decimal amount)
        {
            var methods = new List<PaymentMethod> {PaymentMethod.Cash};
        
            // Add electronic methods for amounts over 10,000 VND
            if (amount >= 10000)
            {
                methods.Add(PaymentMethod.BankTransfer);
            }
            return methods;
        }
    }
}
