using MealPlanner.Domain.Shared;

namespace MealPlanner.Services.Shared;

public static class ServiceErrors
{
    public static class Meal
    {
        public static Error MissingRecipesById(params int[] missingIds) => Error.Validation(TranslationKeys.Meal.MissingRecipesById, ("MissingIds", missingIds));
        public static readonly Error DetailsMissing = Error.Validation(TranslationKeys.Meal.DetailsMissing);
    }

    public static class Menu
    {
        public static readonly Error DoesNotExist = Error.NotFound(TranslationKeys.Menu.DoesNotExist);
        public static readonly Error AlreadyExists = Error.Validation(TranslationKeys.Menu.AlreadyExists);
        public static readonly Error InvalidMeals = Error.Validation(TranslationKeys.Menu.InvalidMeals);
    }

    public static class Recipe
    {
        public static readonly Error DoesNotExist = Error.NotFound(TranslationKeys.Recipe.DoesNotExist);
        public static readonly Error AlreadyExists = Error.Validation(TranslationKeys.Recipe.AlreadyExists);
        public static readonly Error InvalidIngredients = Error.Validation(TranslationKeys.Recipe.InvalidIngredients);
    }

    public static class Ingredient
    {
        public static Error DoesNotExist(params int[] missingIds) =>
            Error.Validation(TranslationKeys.Ingredient.DoesNotExist, ("MissingIds", missingIds));
    }
}