using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSelection.Migrations
{
    public partial class UpdatePlanAddName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubscriptionPlanName",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionPlanName",
                table: "SubscriptionPlans");
        }
    }
}
