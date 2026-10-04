using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CateringCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EventPlan_Add_IngredientsIngoreStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IngredientsIgnoreStock",
                table: "EventPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IngredientsIgnoreStock",
                table: "EventPlans");
        }
    }
}
