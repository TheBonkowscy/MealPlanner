namespace MealPlanner.Domain.Shared;

public static class TranslationKeys
{
    public static class Ingredient
    {
        public const string InvalidQuantity = nameof(InvalidQuantity);
        public const string UnitNotApplicable = nameof(UnitNotApplicable);
        public const string InvalidName = nameof(InvalidName);
        public const string MissingMeasureUnits = nameof(MissingMeasureUnits);
    }

    public static class Recipe
    {
        public const string InvalidName = nameof(InvalidName);
        public const string InvalidServings = nameof(InvalidServings);
        public const string InvalidIngredients = nameof(InvalidIngredients);
        public const string InvalidSteps = nameof(InvalidSteps);
        public const string IsNull = "RecipeIsNull";
    }

    public static class RecipeStep
    {
        public const string InvalidOrder = nameof(InvalidOrder);
        public const string InvalidInstruction = nameof(InvalidInstruction);
        public const string NotFound = "RecipeStepNotFound";
    }

    public static class Meal
    {
        public const string InvalidOrder = nameof(InvalidOrder);
        public const string InvalidServings = nameof(InvalidServings);
        public const string AlreadyExistsAtPosition = nameof(AlreadyExistsAtPosition);
        public const string AlreadyPresentInTheDay = nameof(AlreadyPresentInTheDay);
    }

    public static class Menu
    {
        public const string IsNull = "MenuIsNull";
        public const string InvalidMealOrder = nameof(InvalidMealOrder);
        public const string DateUnset = nameof(DateUnset);
        public const string DateTooFarInThePast = nameof(DateTooFarInThePast);
        public const string DateTooFarInTheFuture = nameof(DateTooFarInTheFuture);
    }
}