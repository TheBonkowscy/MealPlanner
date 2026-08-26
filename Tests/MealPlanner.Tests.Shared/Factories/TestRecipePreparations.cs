using MealPlanner.Domain.Recipes;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestRecipePreparation
{
    public static RecipePreparation Create(string? description = null,
        int? leadDays = null, 
        bool? required = null)
    {
        description ??= $"Description_{Guid.NewGuid().ToString()}";
        leadDays ??= 1;
        required ??= false;
        return RecipePreparation.Create(description,leadDays.Value, required.Value).Value;
    }
}