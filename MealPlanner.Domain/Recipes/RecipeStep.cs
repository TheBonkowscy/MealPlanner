using MealPlanner.Domain.Recipes.Exceptions;

namespace MealPlanner.Domain.Recipes;

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

    public static Result<RecipeStep> Create(int order, string instruction)
    {
        var errors = new List<Error>();
        errors.AddRule(ValidateOrderAndThrow(order), DomainErrors.RecipeStep.InvalidOrder);
        errors.AddRule(ValidateInstructionAndThrow(instruction), DomainErrors.RecipeStep.InvalidInstruction);

        return errors.Count > 0 ? Result.Failure<RecipeStep>(errors) : Result.Success(new RecipeStep(order, instruction));
    }

    private static bool ValidateOrderAndThrow(int order) => order < 1;

    private static bool ValidateInstructionAndThrow(string instructions) => string.IsNullOrWhiteSpace(instructions);

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