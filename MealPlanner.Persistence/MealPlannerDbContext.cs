using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Menus;
using MealPlanner.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Persistence;

public class MealPlannerDbContext : DbContext
{
    public virtual DbSet<Menu> Menus { get; set; }
    public virtual DbSet<Recipe> Recipes { get; set; }
    public virtual DbSet<RecipeStep> RecipeSteps { get; set; }
    public virtual DbSet<Meal> Meals { get; set; }
    public virtual DbSet<Ingredient> Ingredients { get; set; }
    public virtual DbSet<UsedIngredient> UsedIngredients { get; set; }

    public MealPlannerDbContext(DbContextOptions<MealPlannerDbContext> options) : base(options)
    {
    }
    
    protected MealPlannerDbContext()
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MealPlannerDbContext).Assembly);
        modelBuilder.HasPostgresEnum<MeasureUnit>();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
        {
            await IngredientSeeder.SeedAsync(context, cancellationToken);
            await RecipeSeeder.SeedAsync(context, cancellationToken);
        })
        .UseSeeding((context, _) =>
        {
            IngredientSeeder.Seed(context);
            RecipeSeeder.Seed(context);
        });
        
    }
}