
using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Application.Interface.IService;
using CoffeeShop.Application.Interface.IUnitOfWork;
using CoffeeShop.Application.Service;
using CoffeeShop.Application.Service.Gateways;
using CoffeeShop.Infrastructure.Data;
using CoffeeShop.Infrastructure.Repository;
using CoffeeShop.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection;


namespace CoffeeShop.Infrastructure.Extention
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Redis connection multiplexer
            var redisConn = configuration.GetConnectionString("Redis") ?? configuration["Redis:Connection"];
            if (!string.IsNullOrWhiteSpace(redisConn))
            {
                services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));
            }

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
            services.AddScoped<ICafeTableRepository, CafeTableRepository>();
            services.AddScoped<IMenuItemRecipeRepository, MenuItemRecipeRepository>();
            services.AddScoped<ITempOrderRepository, TempOrderRepository>();

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            // Register services
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IBusinessService, BusinessService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IBranchService, BranchService>();

            services.AddScoped<ITableService, TableService>();
            services.AddScoped<IMenuItemService, MenuItemService>();
            services.AddScoped<IIngredientService, IngredientService>();
            services.AddScoped<IQrService, QrService>();
            services.AddScoped<IGuestOrderService, GuestOrderService>();
            services.AddScoped<IPaymentGateway, VNPayGateway>();
            services.AddScoped<IPaymentGateway, MoMoGateway>();


            services.AddHttpContextAccessor();

            return services;
        }
    }
}
