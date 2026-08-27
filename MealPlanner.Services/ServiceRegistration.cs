using MealPlanner.Services.Ingredients;
using MealPlanner.Services.Menus;
using MealPlanner.Services.Recipes;
using MealPlanner.Services.Recipes.Ingredients;
using MealPlanner.Services.Recipes.Preparations;
using MealPlanner.Services.Recipes.Steps;
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
        services.AddTransient<ICreateRecipe, RecipeEditor>();
        services.AddTransient<IDeleteRecipe, RecipeDeleter>();
        services.AddTransient<IUpdateRecipe, RecipeUpdater>();
        
        services.AddTransient<IUpdateRecipeIngredient, RecipeIngredientUpdater>();
        services.AddTransient<IDeleteRecipeIngredient, RecipeIngredientDeleter>();
        
        services.AddTransient<ICreateRecipeStep, RecipeStepCreator>();
        services.AddTransient<IUpdateRecipeStep, RecipeStepUpdater>();
        services.AddTransient<IDeleteRecipeStep, RecipeStepDeleter>();
        
        services.AddTransient<ICreateRecipePreparations, RecipePreparationsCreator>();
        services.AddTransient<IUpdateRecipePreparations, RecipePreparationsUpdater>();
        services.AddTransient<IDeleteRecipePreparations, RecipePreparationsDeleter>();

        services.AddTransient<IReadPreparations, PreparationsReader>();
        
        services.AddSingleton<RecipeMapper>();
        
        /* Ingredients */
        services.AddTransient<IReadIngredient, IngredientReader>();
        
        /* Shared */
        services.AddTransient<MeasureUnitMapper>();
        
        return Task.CompletedTask;
    }
}