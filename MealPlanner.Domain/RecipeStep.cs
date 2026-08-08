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

    private static void ValidateOrderAndThrow(int order)
    {
        if (order < 1)
        {
            throw new ArgumentOutOfRangeException(null, "Order must be greater than 0");
        }
    }

    private static void ValidateInstructionAndThrow(string instruction)
    {
        if (string.IsNullOrWhiteSpace(instruction))
        {
            throw new ArgumentNullException(null, "Instruction cannot be null or whitespace");
        }
    }
}