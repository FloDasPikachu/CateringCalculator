using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CateringCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEventRecipeNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    GuestCount = table.Column<int>(type: "INTEGER", nullable: false),
                    AverageDrinksPerGuest = table.Column<int>(type: "INTEGER", nullable: false),
                    WasteBufferPercent = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Unit = table.Column<int>(type: "INTEGER", nullable: false),
                    PackageSize = table.Column<double>(type: "REAL", nullable: false),
                    PackagePrice = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventRecipe",
                columns: table => new
                {
                    EventPlanId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecipeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Percentage = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRecipe", x => new { x.EventPlanId, x.RecipeId });
                    table.ForeignKey(
                        name: "FK_EventRecipe_EventPlans_EventPlanId",
                        column: x => x.EventPlanId,
                        principalTable: "EventPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventRecipe_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecipeItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IngredientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<double>(type: "REAL", nullable: false),
                    RecipeId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeItems_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeItems_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventRecipe_RecipeId",
                table: "EventRecipe",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeItems_IngredientId",
                table: "RecipeItems",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeItems_RecipeId",
                table: "RecipeItems",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventRecipe");

            migrationBuilder.DropTable(
                name: "RecipeItems");

            migrationBuilder.DropTable(
                name: "EventPlans");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "Recipes");
        }
    }
}
