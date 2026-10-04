using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CateringCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EventPlan_Add_UseStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseStock",
                table: "EventPlans",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseStock",
                table: "EventPlans");
        }
    }
}
