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

            // Configure MoMo payment options
            services.Configure<MomoOption>(config.GetSection("MoMo"));

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

            // Register wallet services
            services.AddScoped<IWalletService, WalletService>();

            return services;
        }

        // Configure HttpClient for external API calls
        public static IServiceCollection ConfigureHttpClients(this IServiceCollection services, IConfiguration config)
        {
            // Configure MoMo HttpClient
            services.AddHttpClient("MoMo", client =>
            {
                var momoEndpoint = config["MoMo:ApiEndpoint"] ?? "https://test-payment.momo.vn";
                client.BaseAddress = new Uri(momoEndpoint);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }
    }
}
