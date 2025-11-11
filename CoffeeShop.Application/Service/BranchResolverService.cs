using CoffeeShop.Application.Interface;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Domain.Enums;

public class BranchResolverService : IBranchResolverService
{
    private readonly IAuthService _authService;

    public BranchResolverService(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<int?> ResolveBranchIdAsync(int? inputBranchId = null)
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null)
            throw new Exception("User not found");

        // Owner dont need branchID
        if (inputBranchId.HasValue && inputBranchId.Value != 0)
            return inputBranchId.Value;
        // Staff must have branchId
        if (user.BranchId.HasValue)
            return user.BranchId.Value;

        throw new Exception("No branch specified");
    }
}
