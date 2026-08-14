using MealPlanner.Domain;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestRecipeSteps
{
    public static RecipeStep Create(int? order = null, string? instructions = null)
    {
        order ??= 1;
        instructions ??= $"Instruction_{Guid.NewGuid().ToString()}";

        return RecipeStep.Create(order.Value, instructions);
    }
    
}