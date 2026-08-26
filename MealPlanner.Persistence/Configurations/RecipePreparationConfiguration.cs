using MealPlanner.Domain.Recipes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Persistence.Configurations;

public class RecipePreparationConfiguration : IEntityTypeConfiguration<RecipePreparation>
{
    public void Configure(EntityTypeBuilder<RecipePreparation> builder)
    {
        builder.ToTable("RecipePreparations");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LeadDays).IsRequired();
        builder.Property(x => x.Description).IsRequired();
    }
}