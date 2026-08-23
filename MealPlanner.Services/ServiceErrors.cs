using MealPlanner.Domain;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Services;

public static class ServiceErrors
{
    public static class Meal
    {
        public static Error MissingRecipesById(params int[] missingIds) => Error.Validation("meal.missing_meals_by_id", ("MissingIds", missingIds));
        public static readonly Error DetailsMissing = Error.Validation("meal.details_missing");
    }

    public static class Menu
    {
        public static readonly Error DoesNotExist = Error.NotFound("menu.does_not_exist");
        public static readonly Error AlreadyExists = Error.Validation("menu.already_exists");
        public static readonly Error InvalidMeals = Error.Validation("menu.invalid_meals");
    }

    public static class Recipe
    {
        public static readonly Error DoesNotExist = Error.NotFound("recipe.does_not_exist");
        public static readonly Error AlreadyExists = Error.Validation("recipe.already_exists");
        public static readonly Error InvalidIngredients = Error.Validation("recipe.invalid_ingredients");
    }

    public static class Ingredient
    {
        public static Error DoesNotExist(params int[] missingIds) =>
            Error.Validation("ingredient.does_not_exist", ("MissingIds", missingIds));
    }
}