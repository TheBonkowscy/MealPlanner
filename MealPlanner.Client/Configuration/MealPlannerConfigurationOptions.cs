namespace MealPlanner.Client.Configuration;

public sealed class MealPlannerConfigurationOptions
{
    public const string SectionName = "MealPlanner";

    public string Host { get; set; } = string.Empty;
    
}