using MealPlanner.Domain.Recipes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Persistence.Configurations;

public class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(EntityTypeBuilder<RecipeStep> builder)
    {
        builder.ToTable("RecipeSteps");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Order).IsRequired();
        builder.Property(x => x.Instructions).IsRequired();
    }
}