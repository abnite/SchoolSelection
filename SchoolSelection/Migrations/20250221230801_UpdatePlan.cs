using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSelection.Migrations
{
    public partial class UpdatePlan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxColleges",
                table: "SubscriptionPlans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxCriteria",
                table: "SubscriptionPlans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxSelections",
                table: "SubscriptionPlans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxSubmissions",
                table: "SubscriptionPlans",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxColleges",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxCriteria",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxSelections",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MaxSubmissions",
                table: "SubscriptionPlans");
        }
    }
}
