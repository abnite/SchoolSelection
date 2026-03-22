using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSelection.Migrations
{
    public partial class UpdatePlan2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FreeUsedColleges",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FreeUsedCriteria",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FreeUsedResults",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FreeUsedSelections",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsedColleges",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsedCriteria",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsedResults",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsedSelections",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreeUsedColleges",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "FreeUsedCriteria",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "FreeUsedResults",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "FreeUsedSelections",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "UsedColleges",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "UsedCriteria",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "UsedResults",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "UsedSelections",
                table: "Subscriptions");
        }
    }
}
