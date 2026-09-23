using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CateringCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventFinancialsAndSalesPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FixedCosts",
                table: "EventPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FreeDrinksCount",
                table: "EventPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PersonnelCosts",
                table: "EventPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetProfit",
                table: "EventPlans",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FixedCosts",
                table: "EventPlans");

            migrationBuilder.DropColumn(
                name: "FreeDrinksCount",
                table: "EventPlans");

            migrationBuilder.DropColumn(
                name: "PersonnelCosts",
                table: "EventPlans");

            migrationBuilder.DropColumn(
                name: "TargetProfit",
                table: "EventPlans");
        }
    }
}
