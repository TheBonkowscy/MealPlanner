using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedShelfLifeToIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShelfLifeDays",
                table: "Ingredients",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShelfLifeDays",
                table: "Ingredients");
        }
    }
}
