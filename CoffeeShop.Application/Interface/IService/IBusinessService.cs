using CoffeeShop.Domain.DTOs;
using CoffeeShop.Domain.Entities;
using CoffeeShop.Domain.Enums;
using CoffeeShop.Application.Common;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IBusinessService
    {
        Task<AdminResult> RegisterBusinessAsync(string businessName, string address, string? phone, int ownerId);

        Task<Business?> GetBusinessByIdAsync(int businessId);
        Task<bool> CompletePaymentAsync(string refCode, PaymentGateway gateway);

        Task<ServiceResult> UpdateBusinessAsync(BusinessEditDto dto);
    }
}


