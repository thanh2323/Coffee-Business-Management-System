
using CoffeeShop.Domain.Enums;
using CoffeeShop.Infrastructure.Extention;
using CoffeeShop.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CoffeeShop.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {

                        options.LoginPath = "/Auth/Login";
                        options.LogoutPath = "/Auth/Logout";
                        options.AccessDeniedPath = "/Auth/Forbidden";

                        options.Cookie.Name = "CoffeeShopAuth";
                        options.Cookie.HttpOnly = true;
                        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        options.Cookie.SameSite = SameSiteMode.Strict;

                        options.ExpireTimeSpan = TimeSpan.FromHours(8);
                        options.SlidingExpiration = true;
                    });
            builder.Services.AddAuthorization(options =>
            {

                // Admin - chỉ có quyền hệ thống
                options.AddPolicy("RequireAdmin", policy =>
                    policy.RequireRole("Admin"));

                // Owner - quản lý business của chính họ
                options.AddPolicy("RequireOwner", policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Owner") &&
                        context.User.HasClaim(c => c.Type == "BusinessId")));

                // Manager - quản lý trong branch
                options.AddPolicy("RequireManager", policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Staff") &&
                        context.User.HasClaim(c => c.Type == "Position" && c.Value == "Manager") &&
                        context.User.HasClaim(c => c.Type == "BranchId")));

                // Staff tác nghiệp
                options.AddPolicy("RequireFrontline", policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Staff") &&
                        context.User.HasClaim(c => c.Type == "Position" &&
                                                  (c.Value == "Barista" || c.Value == "Cashier"))));

                // Owner or Manager (branch-level management)
                options.AddPolicy("RequireOwnerOrManager", policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Owner") ||
                        (context.User.IsInRole("Staff") &&
                         context.User.HasClaim(c => c.Type == "Position" && c.Value == "Manager") &&
                        context.User.HasClaim(c => c.Type == "BranchId"))));
            });

            builder.Services.AddDistributedMemoryCache(); 
            builder.Services.AddSession(options =>
            {
             
                //  options.IdleTimeout = TimeSpan.FromMinutes(90); 
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; 
            });


            var app = builder.Build();

            // Initialize database with seed data
            /*  using (var scope = app.Services.CreateScope())
              {
                  var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                  await DbInitializer.InitializeAsync(context);

                  // Uncomment the line below to seed sample data
                  // await DbInitializer.SeedSampleDataAsync(context);
              }*/

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
