using MealPlanner.Services.Ingredients;
using MealPlanner.Services.Menus;
using MealPlanner.Services.Recipes;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddTransient<IDeleteRecipe, RecipeDeleter>();
        
        /* Ingredients */
        services.AddTransient<IReadIngredient, IngredientReader>();
        
        /* Shared */
        services.AddTransient<MeasureUnitMapper>();
        
        return Task.CompletedTask;
    }
}