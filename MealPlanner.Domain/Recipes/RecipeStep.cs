using MealPlanner.Domain.Recipes.Exceptions;

namespace MealPlanner.Domain;

public class RecipeStep
{
    public int Id { get; private set; }
    public int Order { get; private set; }
    public string Instructions { get; private set; }
    
    private RecipeStep()
    {
        // For EF Core
    }

    private RecipeStep(int order, string instructions)
    {
        Order = order;
        Instructions = instructions;
    }

    public static RecipeStep Create(int order, string instruction)
    {
        ValidateOrderAndThrow(order);
        ValidateInstructionAndThrow(instruction);

        return new RecipeStep(order, instruction);
    }

    private static void ValidateOrderAndThrow(int order) => InvalidStepOrderException.ThrowIfOrderIsInvalid(order);

    private static void ValidateInstructionAndThrow(string instruction) => MissingInstructionsException.ThrowIfInstructionsAreInvalid(instruction);

    public void UpdateOrder(int newOrder)
    {
        ValidateOrderAndThrow(newOrder);
        Order = newOrder;
    }

    public void UpdateInstructions(string newInstructions)
    {
        ValidateInstructionAndThrow(newInstructions);
        Instructions = newInstructions;
    }
}