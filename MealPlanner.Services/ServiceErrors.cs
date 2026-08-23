using MealPlanner.Domain;

namespace MealPlanner.Services;

public static class ServiceErrors
{
    public static class Meal
    {
        public static readonly Error MissingRecipesById = Error.NotFound("meal.missing_meals_by_id");
        public static readonly Error DetailsMissing = Error.NotFound("meal.details_missing");
    }

    public static class Menu
    {
        public static readonly Error DoesNotExist = Error.Conflict("menu.does_not_exist");
        public static readonly Error AlreadyExists = Error.Conflict("menu.already_exists");
        public static readonly Error InvalidMeals = Error.Conflict("menu.invalid_meals");
    }

    public static class Recipe
    {
        public static readonly Error DoesNotExist = Error.Conflict("recipe.does_not_exist");
        public static readonly Error AlreadyExists = Error.Conflict("recipe.already_exists");
        public static readonly Error InvalidIngredients = Error.Conflict("recipe.invalid_ingredients");
    }

    public static class Ingredient
    {
        public static readonly Error DoesNotExist = Error.Conflict("ingredient.does_not_exist");
    }
}