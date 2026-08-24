using MealPlanner.Domain.Shared;

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
        errors.AddRule(ValidateOrder(order), DomainErrors.RecipeStep.InvalidOrder(order))
            .AddRule(ValidateInstruction(instruction), DomainErrors.RecipeStep.InvalidInstruction(instruction));

        return errors.Count > 0 ? Result.Failure<RecipeStep>(errors) : Result.Success(new RecipeStep(order, instruction));
    }

    private static bool ValidateOrder(int order) => order > 0;

    private static bool ValidateInstruction(string instructions) => !string.IsNullOrWhiteSpace(instructions);

    public Result UpdateOrder(int newOrder)
    {
        if (!ValidateOrder(newOrder))
        {
            return Result.Failure(DomainErrors.RecipeStep.InvalidOrder(newOrder));
        }
        Order = newOrder;
        return Result.Success();
    }

    public Result UpdateInstructions(string newInstructions)
    {
        if (!ValidateInstruction(newInstructions))
        {
            return Result.Failure(DomainErrors.RecipeStep.InvalidInstruction(newInstructions));
        }
        Instructions = newInstructions;
        return Result.Success();
    }
}