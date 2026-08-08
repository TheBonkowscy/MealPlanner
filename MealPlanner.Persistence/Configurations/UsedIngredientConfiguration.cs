using MealPlanner.Domain.Ingredients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Persistence.Configurations;

public class UsedIngredientConfiguration : IEntityTypeConfiguration<UsedIngredient>
{
    public void Configure(EntityTypeBuilder<UsedIngredient> builder)
    {
        builder.HasKey(x => new { MealId = x.RecipeId, x.IngredientId, x.UnitId });
        builder.HasOne(x => x.Ingredient).WithMany().HasForeignKey(x => x.IngredientId);
        builder.HasOne(x => x.Recipe).WithMany(x => x.Ingredients).HasForeignKey(x => x.RecipeId);
        
        builder.Metadata.FindNavigation(nameof(UsedIngredient.Recipe))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        builder.Metadata.FindNavigation(nameof(UsedIngredient.Ingredient))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}