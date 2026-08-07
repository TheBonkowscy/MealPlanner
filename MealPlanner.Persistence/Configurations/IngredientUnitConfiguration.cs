using MealPlanner.Domain.Ingredients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Persistence.Configurations;

public class IngredientUnitConfiguration : IEntityTypeConfiguration<IngredientUnit>
{
    public void Configure(EntityTypeBuilder<IngredientUnit> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired();
    }
}