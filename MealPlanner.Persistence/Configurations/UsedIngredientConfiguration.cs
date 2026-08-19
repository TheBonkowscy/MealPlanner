using MealPlanner.Domain.Ingredients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Persistence.Configurations;

public class UsedIngredientConfiguration : IEntityTypeConfiguration<UsedIngredient>
{
    public void Configure(EntityTypeBuilder<UsedIngredient> builder)
    {
        builder.ToTable("UsedIngredients");
        
        builder.HasKey(x => new { x.RecipeId, x.IngredientId, x.Unit });
        
        builder.Property(x => x.Unit).HasConversion<string>();
        
        builder.HasOne(x => x.Recipe).WithMany(x => x.Ingredients).HasForeignKey(x => x.RecipeId);
        builder.HasOne(x => x.Ingredient).WithMany().HasForeignKey(x => x.IngredientId);

        builder.Metadata.FindNavigation(nameof(UsedIngredient.Recipe))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(UsedIngredient.Ingredient))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}