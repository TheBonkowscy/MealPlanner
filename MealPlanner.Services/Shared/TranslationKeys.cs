namespace MealPlanner.Services.Shared;

public static class TranslationKeys
{
    public static class Meal
    {
        public const string MissingRecipesById = nameof(MissingRecipesById);
        public const string DetailsMissing = nameof(DetailsMissing);
    }

    public static class Menu
    {
        public const string DoesNotExist = nameof(DoesNotExist);
        public const string AlreadyExists = nameof(AlreadyExists);
        public const string InvalidMeals = nameof(InvalidMeals);
    }

    public static class Recipe
    {
        public const string DoesNotExist = nameof(DoesNotExist);
        public const string AlreadyExists = nameof(AlreadyExists);
        public const string InvalidIngredients = nameof(InvalidIngredients);
    }

    public static class Ingredient
    {
        public const string DoesNotExist = nameof(DoesNotExist);
    }
}