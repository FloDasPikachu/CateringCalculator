using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CateringCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EventPlan_Rename_IngredientsIngoreStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IngredientsIgnoreStock",
                table: "EventPlans",
                newName: "IngredientsUseStock");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IngredientsUseStock",
                table: "EventPlans",
                newName: "IngredientsIgnoreStock");
        }
    }
}
