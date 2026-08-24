namespace MealPlanner.Services.Shared;

public static class TranslationKeys
{
    public static class Meal
    {
        public const string MissingMealRecipesById = nameof(MissingMealRecipesById);
        public const string MealDetailsMissing = nameof(MealDetailsMissing);
    }

    public static class Menu
    {
        public const string MenuDoesNotExist = nameof(MenuDoesNotExist);
        public const string MenuAlreadyExists = nameof(MenuAlreadyExists);
        public const string InvalidMealsInMenu = nameof(InvalidMealsInMenu);
    }

    public static class Recipe
    {
        public const string RecipeDoesNotExist = nameof(RecipeDoesNotExist);
        public const string RecipeAlreadyExists = nameof(RecipeAlreadyExists);
        public const string InvalidRecipeIngredients = nameof(InvalidRecipeIngredients);
    }

    public static class Ingredient
    {
        public const string IngredientDoesNotExist = nameof(IngredientDoesNotExist);
    }
}