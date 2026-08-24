namespace MealPlanner.Domain.Shared;

public static class TranslationKeys
{
    public static class Ingredient
    {
        public const string InvalidIngredientQuantity = nameof(InvalidIngredientQuantity);
        public const string IngredientUnitNotApplicable = nameof(IngredientUnitNotApplicable);
        public const string InvalidIngredientName = nameof(InvalidIngredientName);
        public const string MissingIngredientMeasureUnits = nameof(MissingIngredientMeasureUnits);
    }

    public static class Recipe
    {
        public const string InvalidRecipeName = nameof(InvalidRecipeName);
        public const string InvalidRecipeServings = nameof(InvalidRecipeServings);
        public const string InvalidRecipeIngredients = nameof(InvalidRecipeIngredients);
        public const string InvalidRecipeSteps = nameof(InvalidRecipeSteps);
        public const string RecipeIsNull = nameof(RecipeIsNull);
    }

    public static class RecipeStep
    {
        public const string InvalidRecipeStepOrder = nameof(InvalidRecipeStepOrder);
        public const string InvalidRecipeStepInstruction = nameof(InvalidRecipeStepInstruction);
        public const string RecipeStepNotFound = nameof(RecipeStepNotFound);
    }

    public static class Meal
    {
        public const string InvalidMealOrder = nameof(InvalidMealOrder);
        public const string InvalidMealServings = nameof(InvalidMealServings);
        public const string MealAlreadyExistsAtPosition = nameof(MealAlreadyExistsAtPosition);
        public const string MealAlreadyPresentInTheDay = nameof(MealAlreadyPresentInTheDay);
    }

    public static class Menu
    {
        public const string MenuIsNull = nameof(MenuIsNull);
        public const string InvalidMealOrderInMenu = nameof(InvalidMealOrderInMenu);
        public const string MenuDateUnset = nameof(MenuDateUnset);
        public const string MenuDateTooFarInThePast = nameof(MenuDateTooFarInThePast);
        public const string MenuDateTooFarInTheFuture = nameof(MenuDateTooFarInTheFuture);
    }
}