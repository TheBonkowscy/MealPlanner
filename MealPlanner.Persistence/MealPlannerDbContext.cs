using MealPlanner.Domain;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Persistence;

public class MealPlannerDbContext : DbContext
{
    public virtual DbSet<Menu> Menus { get; set; }
    public virtual DbSet<Meal> Meals { get; set; }
    public virtual DbSet<MenuItem> MenuItems { get; set; }

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
    }
}