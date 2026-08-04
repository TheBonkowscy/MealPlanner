using MealPlanner.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MealPlanner.Client;

public static class ServiceRegistration
{
    extension(IServiceCollection services) 
    {
        public IServiceCollection AddMealPlannerClient()
        {
            
            services.AddHttpClient<IMenuFinder, MealPlannerClient>(nameof(MealPlannerClient), IServiceCollection.ConfigureClient());
            services.AddHttpClient<IMenuCreator, MealPlannerClient>(nameof(MealPlannerClient), IServiceCollection.ConfigureClient());
            services.AddHttpClient<IMealFinder, MealPlannerClient>(nameof(MealPlannerClient), IServiceCollection.ConfigureClient());
            return services;
        }    
        
        private static Action<IServiceProvider, HttpClient> ConfigureClient() =>
            (services, client) =>
            {
                var options = services.GetRequiredService<IOptions<MealPlannerConfigurationOptions>>();

                client.BaseAddress = new Uri(options.Value.Host);
                
                // TODO: resiliency
            };
    }
}