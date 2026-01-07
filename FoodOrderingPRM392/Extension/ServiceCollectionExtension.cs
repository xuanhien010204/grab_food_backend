using FoodOrderingCore.ConfigurationOptions;
using FoodOrderingRepository.Implement;
using FoodOrderingRepository.Interface;

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
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IFoodTypeRepository, FoodTypeRepository>();
            services.AddScoped<IFoodStoreRepository, FoodStoreRepository>();
            return services;
        }
    }
}
