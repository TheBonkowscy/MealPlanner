using MealPlanner.Domain.Shared;

namespace MealPlanner.Services.Shared;

public static class ServiceErrors
{
    public static class Meal
    {
        public static Error MissingRecipesById(params int[] missingIds) => Error.Validation(TranslationKeys.Meal.MissingMealRecipesById, ("MissingIds", missingIds));
        public static readonly Error NothingToUpdateForMeal = Error.Validation(TranslationKeys.Meal.NothingToUpdateForMeal);
        public static readonly Error TransferToTheSameDay = Error.Validation(TranslationKeys.Meal.TransferToTheSameDay);
    }

    public static class Menu
    {
        public static Error DoesNotExist(DateOnly date) =>
            Error.NotFound(TranslationKeys.Menu.MenuDoesNotExist, ("Date", date));
    }

    public static class Recipe
    {
        public static readonly Error DoesNotExist = Error.NotFound(TranslationKeys.Recipe.RecipeDoesNotExist);
        public static readonly Error AlreadyExists = Error.Validation(TranslationKeys.Recipe.RecipeAlreadyExists);
        public static readonly Error InvalidIngredients = Error.Validation(TranslationKeys.Recipe.InvalidRecipeIngredients);
    }

    public static class Ingredient
    {
        public static Error DoesNotExist(params int[] missingIds) =>
            Error.Validation(TranslationKeys.Ingredient.IngredientDoesNotExist, ("MissingIds", missingIds));
    }

    public static class Shopping
    {
        public static Error InvertedDateRange(DateOnly from, DateOnly to) =>
        Error.Validation(TranslationKeys.Shopping.InvertedDateRange, 
            ("From", from), 
            ("To", to));
    }
}