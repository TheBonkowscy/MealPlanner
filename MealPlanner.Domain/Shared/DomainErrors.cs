using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Domain.Shared;

public static class DomainErrors
{
    public static class Ingredient
    {
        public static Error InvalidQuantity(decimal quantity, string ingredientName) =>
            Error.Validation(TranslationKeys.Ingredient.InvalidIngredientQuantity, 
                ("Quantity", quantity),
                ("IngredientName", ingredientName));

        public static Error UnitNotApplicable(MeasureUnit unit, string ingredientName) =>
            Error.Validation(TranslationKeys.Ingredient.IngredientUnitNotApplicable, 
                ("IngredientName", ingredientName),
                ("Unit", unit));
        public static Error InvalidName(string name) => Error.Validation(TranslationKeys.Ingredient.InvalidIngredientName, ("Name", name));

        public static Error MissingMeasureUnits(string ingredientName) => Error.Validation(
            TranslationKeys.Ingredient.MissingIngredientMeasureUnits, ("IngredientName", ingredientName));
    }

    public static class Recipe
    {
        public static Error InvalidName(string name) => Error.Validation(TranslationKeys.Recipe.InvalidRecipeName, ("Name", name));
        public static Error InvalidServings(int servings) => Error.Validation(TranslationKeys.Recipe.InvalidRecipeServings, ("Servings", servings));
        
        // TODO: maybe change this to include the invalid ingredients at some point
        public static readonly Error InvalidRecipeIngredients = Error.Validation(TranslationKeys.Recipe.InvalidRecipeIngredients);
        
        // TODO: maybe change this to include the details -> no steps at all? Duplicate order?
        public static readonly Error InvalidSteps = Error.Validation(TranslationKeys.Recipe.InvalidRecipeSteps);
        
        public static readonly Error IsNull = Error.Validation(TranslationKeys.Recipe.RecipeIsNull);
    }

    public static class RecipeStep
    {
        public static Error InvalidOrder(int order) => Error.Validation(TranslationKeys.RecipeStep.InvalidRecipeStepOrder, ("Order", order));
        public static Error InvalidInstruction(string instruction) => Error.Validation(TranslationKeys.RecipeStep.InvalidRecipeStepInstruction, ("Instruction", instruction));
        public static readonly Error NotFound = Error.NotFound(TranslationKeys.RecipeStep.RecipeStepNotFound);
    }

    public static class Meal
    {
        public static Error InvalidOrder(int order) => Error.Validation(TranslationKeys.Meal.InvalidMealOrder, ("Order", order));
        public static Error InvalidServings(int servings) => Error.Validation(TranslationKeys.Meal.InvalidMealServings, ("Servings", servings));
        public static Error AlreadyExistsAtPosition(int order) =>
            Error.Validation(TranslationKeys.Meal.MealAlreadyExistsAtPosition, ("Order", order));
        public static Error AlreadyPresentInTheDay(string mealName) =>
            Error.Validation(TranslationKeys.Meal.MealAlreadyPresentInTheDay, ("MealName", mealName));
    }

    public static class Menu
    {
        public static readonly Error IsNull = Error.Validation(TranslationKeys.Menu.MenuIsNull);
        public static readonly Error InvalidMealOrder = Error.Validation(TranslationKeys.Menu.InvalidMealOrderInMenu);
        public static readonly Error DateIsUnset = Error.Validation(TranslationKeys.Menu.MenuDateUnset);
        public static readonly Error DateTooFarInThePast = Error.Validation(TranslationKeys.Menu.MenuDateTooFarInThePast);
        public static readonly Error DateTooFarInTheFuture = Error.Validation(TranslationKeys.Menu.MenuDateTooFarInTheFuture);
    }
}