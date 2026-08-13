using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDataSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:measure_unit", "glass_cup,tablespoon,teaspoon,milligram,gram,kilogram,milliliter,liter,piece,package,can,bottle,pinch,slice1,slice2")
                .OldAnnotation("Npgsql:Enum:measure_unit", "glass_cup,tablespoon,teaspoon,milligram,gram,kilogram,milliliter,liter,piece,package,can,bottle,pinch");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:measure_unit", "glass_cup,tablespoon,teaspoon,milligram,gram,kilogram,milliliter,liter,piece,package,can,bottle,pinch")
                .OldAnnotation("Npgsql:Enum:measure_unit", "glass_cup,tablespoon,teaspoon,milligram,gram,kilogram,milliliter,liter,piece,package,can,bottle,pinch,slice1,slice2");
        }
    }
}
