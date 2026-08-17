using MealPlanner.Domain;
using MealPlanner.Domain.Menus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Persistence.Configurations;

public class MealConfiguration : IEntityTypeConfiguration<Meal>
{
    public void Configure(EntityTypeBuilder<Meal> builder)
    {
        builder.ToTable("Meals");
        
        builder.HasKey(x => new { x.MenuId, x.RecipeId });
        
        builder.Property(x => x.Order).IsRequired();
        builder.Property(x => x.Servings).IsRequired();
    }
}