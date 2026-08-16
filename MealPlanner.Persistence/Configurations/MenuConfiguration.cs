using MealPlanner.Domain;
using MealPlanner.Domain.Menus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MealPlanner.Persistence.Configurations;

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Date).IsRequired();
        
        builder.HasMany(x => x.Meals)
            .WithOne(x => x.Menu)
            .HasForeignKey(x => x.MenuId);
        
        builder.Metadata.FindNavigation(nameof(Menu.Meals))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        
        builder.HasIndex(x => x.Date).IsUnique();
    }
}