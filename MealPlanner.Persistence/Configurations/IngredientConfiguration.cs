using MealPlanner.Domain.Ingredients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Persistence.Configurations;

public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired();
        builder.HasMany(x => x.ApplicableUnits).WithOne().HasForeignKey(x => x.Id);
        
        
        builder.HasMany(x => x.ApplicableUnits)
            .WithOne();
        
        builder.Metadata.FindNavigation(nameof(Ingredient.ApplicableUnits))!.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}