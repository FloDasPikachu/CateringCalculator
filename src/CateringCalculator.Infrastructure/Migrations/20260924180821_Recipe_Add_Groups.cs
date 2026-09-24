using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CateringCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Recipe_Add_Groups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Groups",
                table: "Recipes",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Groups",
                table: "Recipes");
        }
    }
}
