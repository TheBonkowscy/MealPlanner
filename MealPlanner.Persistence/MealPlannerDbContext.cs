using MealPlanner.Domain;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Persistence;

public class MealPlannerDbContext(DbContextOptions<MealPlannerDbContext> options) : DbContext(options)
{
    public DbSet<Menu> Menus { get; set; }
    public DbSet<Meal> Meals { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MealPlannerDbContext).Assembly);
    }
}