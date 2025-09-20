using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Domain.Enums
{
    public enum PaymentStatus
    {
        Unpaid = 0,
        Pending = 1,
        Completed = 2,
        Failed = 3,
       
    }
}
