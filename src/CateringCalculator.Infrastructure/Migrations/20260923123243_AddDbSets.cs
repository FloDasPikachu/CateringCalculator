using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CateringCalculator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDbSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FixedCostItem_EventPlans_EventPlanId",
                table: "FixedCostItem");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonnelCostItem_EventPlans_EventPlanId",
                table: "PersonnelCostItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonnelCostItem",
                table: "PersonnelCostItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FixedCostItem",
                table: "FixedCostItem");

            migrationBuilder.RenameTable(
                name: "PersonnelCostItem",
                newName: "PersonnelCostItems");

            migrationBuilder.RenameTable(
                name: "FixedCostItem",
                newName: "FixedCostItems");

            migrationBuilder.RenameIndex(
                name: "IX_PersonnelCostItem_EventPlanId",
                table: "PersonnelCostItems",
                newName: "IX_PersonnelCostItems_EventPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_FixedCostItem_EventPlanId",
                table: "FixedCostItems",
                newName: "IX_FixedCostItems_EventPlanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonnelCostItems",
                table: "PersonnelCostItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FixedCostItems",
                table: "FixedCostItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FixedCostItems_EventPlans_EventPlanId",
                table: "FixedCostItems",
                column: "EventPlanId",
                principalTable: "EventPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonnelCostItems_EventPlans_EventPlanId",
                table: "PersonnelCostItems",
                column: "EventPlanId",
                principalTable: "EventPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FixedCostItems_EventPlans_EventPlanId",
                table: "FixedCostItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonnelCostItems_EventPlans_EventPlanId",
                table: "PersonnelCostItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonnelCostItems",
                table: "PersonnelCostItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FixedCostItems",
                table: "FixedCostItems");

            migrationBuilder.RenameTable(
                name: "PersonnelCostItems",
                newName: "PersonnelCostItem");

            migrationBuilder.RenameTable(
                name: "FixedCostItems",
                newName: "FixedCostItem");

            migrationBuilder.RenameIndex(
                name: "IX_PersonnelCostItems_EventPlanId",
                table: "PersonnelCostItem",
                newName: "IX_PersonnelCostItem_EventPlanId");

            migrationBuilder.RenameIndex(
                name: "IX_FixedCostItems_EventPlanId",
                table: "FixedCostItem",
                newName: "IX_FixedCostItem_EventPlanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonnelCostItem",
                table: "PersonnelCostItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FixedCostItem",
                table: "FixedCostItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FixedCostItem_EventPlans_EventPlanId",
                table: "FixedCostItem",
                column: "EventPlanId",
                principalTable: "EventPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonnelCostItem_EventPlans_EventPlanId",
                table: "PersonnelCostItem",
                column: "EventPlanId",
                principalTable: "EventPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
