
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Application.Service;
using CoffeeShop.Application.Service.Gateways;
using CoffeeShop.Infrastructure.Data;
using CoffeeShop.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.DependencyInjection;


namespace CoffeeShop.Infrastructure.Extention
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Register repositories
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IBranchRepository, BranchRepository>();
            services.AddScoped<IBusinessRepository, BusinessRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IStaffProfileRepository, StaffProfileRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IMenuItemRepository, MenuItemRepository>();
            services.AddScoped<IIngredientRepository, IngredientRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
    //        services.AddScoped<ICafeTableRepository, CafeTableRepository>();

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            // Register services
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IBusinessService, BusinessService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IBranchService, BranchService>();

            services.AddScoped<ITableService, TableService>();
            services.AddScoped<IPaymentGateway, VNPayGateway>();
            services.AddScoped<IPaymentGateway, MoMoGateway>();


            services.AddHttpContextAccessor();

            return services;
        }
    }
}
