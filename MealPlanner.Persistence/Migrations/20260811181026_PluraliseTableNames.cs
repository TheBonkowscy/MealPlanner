using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPlanner.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PluraliseTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeStep_Recipes_RecipeId",
                table: "RecipeStep");

            migrationBuilder.DropForeignKey(
                name: "FK_UsedIngredient_Ingredient_IngredientId",
                table: "UsedIngredient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecipeStep",
                table: "RecipeStep");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ingredient",
                table: "Ingredient");

            migrationBuilder.RenameTable(
                name: "RecipeStep",
                newName: "RecipeSteps");

            migrationBuilder.RenameTable(
                name: "Ingredient",
                newName: "Ingredients");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeStep_RecipeId",
                table: "RecipeSteps",
                newName: "IX_RecipeSteps_RecipeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecipeSteps",
                table: "RecipeSteps",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ingredients",
                table: "Ingredients",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeSteps_Recipes_RecipeId",
                table: "RecipeSteps",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsedIngredient_Ingredients_IngredientId",
                table: "UsedIngredient",
                column: "IngredientId",
                principalTable: "Ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeSteps_Recipes_RecipeId",
                table: "RecipeSteps");

            migrationBuilder.DropForeignKey(
                name: "FK_UsedIngredient_Ingredients_IngredientId",
                table: "UsedIngredient");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecipeSteps",
                table: "RecipeSteps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ingredients",
                table: "Ingredients");

            migrationBuilder.RenameTable(
                name: "RecipeSteps",
                newName: "RecipeStep");

            migrationBuilder.RenameTable(
                name: "Ingredients",
                newName: "Ingredient");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeSteps_RecipeId",
                table: "RecipeStep",
                newName: "IX_RecipeStep_RecipeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecipeStep",
                table: "RecipeStep",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ingredient",
                table: "Ingredient",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeStep_Recipes_RecipeId",
                table: "RecipeStep",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsedIngredient_Ingredient_IngredientId",
                table: "UsedIngredient",
                column: "IngredientId",
                principalTable: "Ingredient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
