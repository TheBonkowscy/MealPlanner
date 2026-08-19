using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PluraliseNewTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsedIngredient_Ingredients_IngredientId",
                table: "UsedIngredient");

            migrationBuilder.DropForeignKey(
                name: "FK_UsedIngredient_Recipes_RecipeId",
                table: "UsedIngredient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsedIngredient",
                table: "UsedIngredient");

            migrationBuilder.RenameTable(
                name: "UsedIngredient",
                newName: "UsedIngredients");

            migrationBuilder.RenameIndex(
                name: "IX_UsedIngredient_IngredientId",
                table: "UsedIngredients",
                newName: "IX_UsedIngredients_IngredientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsedIngredients",
                table: "UsedIngredients",
                columns: new[] { "RecipeId", "IngredientId", "Unit" });

            migrationBuilder.AddForeignKey(
                name: "FK_UsedIngredients_Ingredients_IngredientId",
                table: "UsedIngredients",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsedIngredients_Recipes_RecipeId",
                table: "UsedIngredients",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsedIngredients_Ingredients_IngredientId",
                table: "UsedIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_UsedIngredients_Recipes_RecipeId",
                table: "UsedIngredients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsedIngredients",
                table: "UsedIngredients");

            migrationBuilder.RenameTable(
                name: "UsedIngredients",
                newName: "UsedIngredient");

            migrationBuilder.RenameIndex(
                name: "IX_UsedIngredients_IngredientId",
                table: "UsedIngredient",
                newName: "IX_UsedIngredient_IngredientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsedIngredient",
                table: "UsedIngredient",
                columns: new[] { "RecipeId", "IngredientId", "Unit" });

            migrationBuilder.AddForeignKey(
                name: "FK_UsedIngredient_Ingredients_IngredientId",
                table: "UsedIngredient",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UsedIngredient_Recipes_RecipeId",
                table: "UsedIngredient",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
