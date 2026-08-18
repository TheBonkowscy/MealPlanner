using MealPlanner.Client.Configuration;
using MealPlanner.Client.Menus;
using MealPlanner.Client.Recipes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MealPlanner.Client;

public static class ServiceRegistration
{
    extension(IServiceCollection services) 
    {
        public IServiceCollection AddMealPlannerClient()
        {
            
            services.AddHttpClient<IFindMenus, MenuClient>(nameof(MenuClient), IServiceCollection.ConfigureClient());
            services.AddHttpClient<ICreateMenus, MenuClient>(nameof(MenuClient), IServiceCollection.ConfigureClient());
            services.AddHttpClient<IUpdateMenus, MenuClient>(nameof(MenuClient), IServiceCollection.ConfigureClient());
            services.AddHttpClient<IDeleteMenus, MenuClient>(nameof(MenuClient), IServiceCollection.ConfigureClient());
            
            services.AddHttpClient<IFindRecipes, RecipeClient>(nameof(RecipeClient), IServiceCollection.ConfigureClient());
            services.AddHttpClient<ICreateRecipes, RecipeClient>(nameof(RecipeClient), IServiceCollection.ConfigureClient());
            services.AddHttpClient<IUpdateRecipes, RecipeClient>(nameof(RecipeClient), IServiceCollection.ConfigureClient());
            services.AddHttpClient<IDeleteRecipes, RecipeClient>(nameof(RecipeClient), IServiceCollection.ConfigureClient());
            
            services.AddHttpClient<IFindIngredients, RecipeClient>(nameof(RecipeClient), IServiceCollection.ConfigureClient());
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