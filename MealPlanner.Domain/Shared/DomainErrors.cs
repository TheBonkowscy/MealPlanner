using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Domain.Shared;

public static class DomainErrors
{
    public static class Ingredient
    {
        public static Error InvalidQuantity(decimal quantity) => Error.Validation(TranslationKeys.Ingredient.InvalidQuantity, ("Quantity", quantity));
        public static Error UnitNotApplicable(MeasureUnit unit) => Error.Validation(TranslationKeys.Ingredient.UnitNotApplicable, ("Unit", unit));
        public static Error InvalidName(string name) => Error.Validation(TranslationKeys.Ingredient.InvalidName, ("Name", name));
        public static readonly Error MissingMeasureUnits = Error.Validation(TranslationKeys.Ingredient.MissingMeasureUnits);
    }

    public static class Recipe
    {
        public static Error InvalidName(string name) => Error.Validation(TranslationKeys.Recipe.InvalidName, ("Name", name));
        public static Error InvalidServings(int servings) => Error.Validation(TranslationKeys.Recipe.InvalidServings, ("Servings", servings));
        
        // TODO: maybe change this to include the invalid ingredients at some point
        public static readonly Error InvalidIngredients = Error.Validation(TranslationKeys.Recipe.InvalidIngredients);
        
        // TODO: maybe change this to include the details -> no steps at all? Duplicate order?
        public static readonly Error InvalidSteps = Error.Validation(TranslationKeys.Recipe.InvalidSteps);
        
        public static readonly Error IsNull = Error.Validation(TranslationKeys.Recipe.IsNull);
    }

    public static class RecipeStep
    {
        public static Error InvalidOrder(int order) => Error.Validation(TranslationKeys.RecipeStep.InvalidOrder, ("Order", order));
        public static Error InvalidInstruction(string instruction) => Error.Validation(TranslationKeys.RecipeStep.InvalidInstruction, ("Instruction", instruction));
        public static readonly Error NotFound = Error.NotFound(TranslationKeys.RecipeStep.NotFound);
    }

    public static class Meal
    {
        public static Error InvalidOrder(int order) => Error.Validation(TranslationKeys.Meal.InvalidOrder, ("Order", order));
        public static Error InvalidServings(int servings) => Error.Validation(TranslationKeys.Meal.InvalidServings, ("Servings", servings));
        public static readonly Error AlreadyExistsAtPosition = Error.Validation(TranslationKeys.Meal.AlreadyExistsAtPosition);
        public static readonly Error AlreadyPresentInTheDay = Error.Validation(TranslationKeys.Meal.AlreadyPresentInTheDay);
    }

    public static class Menu
    {
        public static readonly Error IsNull = Error.Validation(TranslationKeys.Menu.IsNull);
        public static readonly Error InvalidMealOrder = Error.Validation(TranslationKeys.Menu.InvalidMealOrder);
        public static readonly Error DateIsUnset = Error.Validation(TranslationKeys.Menu.DateUnset);
        public static readonly Error DateTooFarInThePast = Error.Validation(TranslationKeys.Menu.DateTooFarInThePast);
        public static readonly Error DateTooFarInTheFuture = Error.Validation(TranslationKeys.Menu.DateTooFarInTheFuture);
    }
}