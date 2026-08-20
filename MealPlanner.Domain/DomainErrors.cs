namespace MealPlanner.Domain;

public static class DomainErrors
{
    public static class Ingredients
    {
        public static readonly Error InvalidQuantity = Error.Validation("ingredients.invalid_quantity");
    }

    public static class Recipe
    {
        public static readonly Error InvalidName = Error.Validation("recipe.invalid_name");
        public static readonly Error InvalidServings = Error.Validation("recipe.invalid_servings");
        public static readonly Error InvalidIngredients = Error.Validation("recipe.invalid_ingredients");
        public static readonly Error InvalidSteps = Error.Validation("recipe.invalid_steps");
    }

    public static class RecipeStep
    {
        public static readonly Error InvalidOrder  = Error.Validation("recipe_step.invalid_order");
        public static readonly Error InvalidInstruction = Error.Validation("recipe_step.invalid_instruction");
    }
}