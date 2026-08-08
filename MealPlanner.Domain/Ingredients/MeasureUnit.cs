namespace MealPlanner.Domain.Ingredients;

public class MeasureUnit
{
    public int Id { get; private set; }
    public string Name { get; private set; }

    private MeasureUnit()
    {
        // For EF Core
    }

    public static MeasureUnit Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(null, "Name cannot be null or whitespace");
        }

        return new MeasureUnit
        {
            Name = name
        };
    }
}