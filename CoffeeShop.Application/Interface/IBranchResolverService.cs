using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Application.Interface
{
    public interface IBranchResolverService
    {
        Task<int?> ResolveBranchIdAsync(int? inputBranchId = null);
    }
}
