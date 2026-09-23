using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CateringCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailedPersonnelCosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FixedCostItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventPlanId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FixedCostItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FixedCostItem_EventPlans_EventPlanId",
                        column: x => x.EventPlanId,
                        principalTable: "EventPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonnelCostItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventPlanId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RoleName = table.Column<string>(type: "TEXT", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false),
                    Hours = table.Column<decimal>(type: "TEXT", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnelCostItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonnelCostItem_EventPlans_EventPlanId",
                        column: x => x.EventPlanId,
                        principalTable: "EventPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FixedCostItem_EventPlanId",
                table: "FixedCostItem",
                column: "EventPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelCostItem_EventPlanId",
                table: "PersonnelCostItem",
                column: "EventPlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FixedCostItem");

            migrationBuilder.DropTable(
                name: "PersonnelCostItem");
        }
    }
}
