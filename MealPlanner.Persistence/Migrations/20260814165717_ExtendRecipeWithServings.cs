using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExtendRecipeWithServings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeSteps_Recipes_RecipeId",
                table: "RecipeSteps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsedIngredient",
                table: "UsedIngredient");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "UsedIngredient");

            migrationBuilder.AddColumn<int>(
                name: "Servings",
                table: "Recipes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsedIngredient",
                table: "UsedIngredient",
                columns: new[] { "RecipeId", "IngredientId", "Unit" });

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeSteps_Recipes_RecipeId",
                table: "RecipeSteps",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeSteps_Recipes_RecipeId",
                table: "RecipeSteps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsedIngredient",
                table: "UsedIngredient");

            migrationBuilder.DropColumn(
                name: "Servings",
                table: "Recipes");

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "UsedIngredient",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsedIngredient",
                table: "UsedIngredient",
                columns: new[] { "RecipeId", "IngredientId", "UnitId" });

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeSteps_Recipes_RecipeId",
                table: "RecipeSteps",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id");
        }
    }
}
