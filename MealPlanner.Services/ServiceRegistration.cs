using MealPlanner.Services.Ingredients;
using MealPlanner.Services.Menus;
using MealPlanner.Services.Recipes;
using MealPlanner.Services.Recipes.Ingredients;
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
        
        /* Meals */
        services.AddTransient<IMapMeals, MealsMapper>();

        /* Recipes */
        services.AddTransient<IReadRecipe, RecipeReader>();
        services.AddTransient<ICreateRecipe, RecipeCreator>();
        services.AddTransient<IDeleteRecipe, RecipeDeleter>();
        services.AddTransient<IUpdateRecipe, RecipeUpdater>();
        services.AddTransient<IUpdateRecipeIngredient, RecipeIngredientUpdater>();
        services.AddTransient<IDeleteRecipeIngredient, RecipeIngredientDeleter>();
        services.AddSingleton<RecipeMapper>();
        
        /* Ingredients */
        services.AddTransient<IReadIngredient, IngredientReader>();
        
        /* Shared */
        services.AddTransient<MeasureUnitMapper>();
        
        return Task.CompletedTask;
    }
}