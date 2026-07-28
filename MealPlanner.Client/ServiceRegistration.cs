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
            services.AddHttpClient(nameof(MealPlannerClient), (services, client) =>
            {
                var options = services.GetRequiredService<IOptions<MealPlannerConfigurationOptions>>();

                client.BaseAddress = new Uri(options.Value.Host);
                
                // TODO: resiliency
            });
            services.AddTransient<IMenuClient, MealPlannerClient>();
            return services;
        }        
    }
}