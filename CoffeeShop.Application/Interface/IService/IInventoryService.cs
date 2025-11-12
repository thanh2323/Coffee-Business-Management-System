using CoffeeShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Application.Interface.IService
{
    public interface IInventoryService
    {
        Task DeductInventoryFromOrderAsync(Order order);
    }
}

