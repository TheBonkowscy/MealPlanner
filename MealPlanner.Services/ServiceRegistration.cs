using MealPlanner.Domain.Shopping.Planning;
using MealPlanner.Services.Ingredients;
using MealPlanner.Services.Menus;
using MealPlanner.Services.Menus.Meals;
using MealPlanner.Services.Recipes;
using MealPlanner.Services.Recipes.Ingredients;
using MealPlanner.Services.Recipes.Preparations;
using MealPlanner.Services.Recipes.Steps;
using MealPlanner.Services.Shopping;
using MealPlanner.Services.Shopping.Adapters;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Services;

public static class ServiceRegistration
{
    public static Task RegisterMenuServices(this IServiceCollection services)
    {
        /* Menus */
        services.AddTransient<IReadMenu, MenuReader>();
        services.AddTransient<IDeleteMenu, MenuDeleter>();

        /* Meals */
        services.AddTransient<ICreateMeals, MealCreator>();
        services.AddTransient<IUpdateMeals, MealUpdater>();
        services.AddTransient<IDeleteMeals, MealDeleter>();
        services.AddTransient<ITransferMeals, MealTransferer>();

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
        
        /* Shopping */
        services.AddTransient<IShoppingNeedsSource, MealPlanningNeedsAdapter>();
        services.AddSingleton<IClosedDaysProvider>(_ => new StaticClosedDaysProvider([.. ClosedDaysCalendar.Dates]));
        services.AddSingleton<IPlanShoppingTrips, ShoppingTripPlanner>();
        services.AddTransient<ShoppingListGenerator>();
        
        return Task.CompletedTask;
    }
}