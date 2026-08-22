namespace MealPlanner.Domain;

public static class DomainErrors
{
    public static class Ingredients
    {
        public static readonly Error InvalidQuantity = Error.Validation("ingredients.invalid_quantity");
        public static readonly Error UnitNotApplicable = Error.Validation("ingredients.unit_not_applicable");
        public static readonly Error InvalidName =  Error.Validation("ingredients.invalid_name");
        public static readonly Error MissingMeasureUnits =  Error.Validation("ingredients.missing_measure_units");
    }

    public static class Recipe
    {
        public static readonly Error InvalidName = Error.Validation("recipe.invalid_name");
        public static readonly Error InvalidServings = Error.Validation("recipe.invalid_servings");
        public static readonly Error InvalidIngredients = Error.Validation("recipe.invalid_ingredients");
        public static readonly Error InvalidSteps = Error.Validation("recipe.invalid_steps");
        public static readonly Error IsNull = Error.Validation("recipe.is_null");
    }

    public static class RecipeStep
    {
        public static readonly Error InvalidOrder  = Error.Validation("recipe_step.invalid_order");
        public static readonly Error InvalidInstruction = Error.Validation("recipe_step.invalid_instruction");
        public static readonly Error NotFound = Error.NotFound("recipe_step.not_found");
    }

    public static class Meal
    {
        public static readonly Error InvalidOrder = Error.Validation("meal.invalid_order");
        public static readonly Error InvalidServings = Error.Validation("meal.invalid_servings");
        public static readonly Error AlreadyExistsAtPosition = Error.Validation("meal.already_exists_at_position");
        public static readonly Error AlreadyPresentInTheDay = Error.Validation("meal.already_present_in_the_day");
    }

    public static class Menu
    {
        public static readonly Error IsNull = Error.Validation("menu.is_null");
        public static readonly Error InvalidMealOrder = Error.Validation("menu.invalid_meal_order");
        public static readonly Error DateIsUnset = Error.Validation("menu.date_is_unset");
        public static readonly Error DateTooFarInThePast = Error.Validation("menu.date_too_far_in_the_past");
        public static readonly Error DateTooFarInTheFuture = Error.Validation("menu.date_too_far_in_the_future");
    }
}