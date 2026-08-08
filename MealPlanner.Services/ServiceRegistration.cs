using MealPlanner.Services.Ingredients;
using MealPlanner.Services.Menus;
using Microsoft.Extensions.DependencyInjection;
using MealPlanner.Services.Recipes;

namespace MealPlanner.Services;

public static class ServiceRegistration
{
    public static Task RegisterMenuServices(this IServiceCollection services)
    {
        /* Menus */
        services.AddTransient<ICreateMenu, MenuCreator>();
        services.AddTransient<IReadMenu, MenuReader>();
        services.AddTransient<IUpdateMenu, MenuUpdater>();
        services.AddTransient<IDeleteMenu, MenuDeleter>();
        
        /* Meals & Recipes */
        services.AddTransient<IReadRecipe, RecipeReader>();
        services.AddTransient<ICreateRecipe, RecipeCreator>();
        services.AddTransient<IMapRecipe, RecipeMapper>();
        
        /* Ingredients */
        services.AddTransient<IReadIngredient, IngredientReader>();
        
        return Task.CompletedTask;
    }
}