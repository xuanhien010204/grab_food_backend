using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingCore.Data;
using FoodOrderingRepository.Implement;
using FoodOrderingRepository.Interface;
using Microsoft.AspNetCore.Identity;

namespace FoodOrderingPRM392.Extension
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection ConfigureApplicationOptions(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ConnectionOption>(config.GetSection("ConnectionStrings"));

            return services;
        }

        public static IServiceCollection ConfigureRepositories(this IServiceCollection services)
        {
            // Register PasswordHasher for password hashing
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // Register dependency inject
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IFoodTypeRepository, FoodTypeRepository>();
            services.AddScoped<IFoodStoreRepository, FoodStoreRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IFoodRepository, FoodRepository>();
            return services;
        }
    }
}
