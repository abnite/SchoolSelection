using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSelection.Migrations
{
    public partial class UPDATEUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddedById",
                table: "SelectionResults",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "SelectionResults",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddedById",
                table: "Schools",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "Schools",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddedById",
                table: "Evaluations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "Evaluations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddedById",
                table: "Criteria",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "Criteria",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AddedById",
                table: "CollegeSelections",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "CollegeSelections",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FreeUsed",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Introduction",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SelectionResults_AddedById",
                table: "SelectionResults",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_SelectionResults_DeletedById",
                table: "SelectionResults",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_AddedById",
                table: "Schools",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_DeletedById",
                table: "Schools",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_AddedById",
                table: "Evaluations",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_DeletedById",
                table: "Evaluations",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_Criteria_AddedById",
                table: "Criteria",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_Criteria_DeletedById",
                table: "Criteria",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeSelections_AddedById",
                table: "CollegeSelections",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeSelections_DeletedById",
                table: "CollegeSelections",
                column: "DeletedById");

            migrationBuilder.AddForeignKey(
                name: "FK_CollegeSelections_AspNetUsers_AddedById",
                table: "CollegeSelections",
                column: "AddedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CollegeSelections_AspNetUsers_DeletedById",
                table: "CollegeSelections",
                column: "DeletedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Criteria_AspNetUsers_AddedById",
                table: "Criteria",
                column: "AddedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Criteria_AspNetUsers_DeletedById",
                table: "Criteria",
                column: "DeletedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_AspNetUsers_AddedById",
                table: "Evaluations",
                column: "AddedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_AspNetUsers_DeletedById",
                table: "Evaluations",
                column: "DeletedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_AspNetUsers_AddedById",
                table: "Schools",
                column: "AddedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_AspNetUsers_DeletedById",
                table: "Schools",
                column: "DeletedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SelectionResults_AspNetUsers_AddedById",
                table: "SelectionResults",
                column: "AddedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SelectionResults_AspNetUsers_DeletedById",
                table: "SelectionResults",
                column: "DeletedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CollegeSelections_AspNetUsers_AddedById",
                table: "CollegeSelections");

            migrationBuilder.DropForeignKey(
                name: "FK_CollegeSelections_AspNetUsers_DeletedById",
                table: "CollegeSelections");

            migrationBuilder.DropForeignKey(
                name: "FK_Criteria_AspNetUsers_AddedById",
                table: "Criteria");

            migrationBuilder.DropForeignKey(
                name: "FK_Criteria_AspNetUsers_DeletedById",
                table: "Criteria");

            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_AspNetUsers_AddedById",
                table: "Evaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_AspNetUsers_DeletedById",
                table: "Evaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_AspNetUsers_AddedById",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_Schools_AspNetUsers_DeletedById",
                table: "Schools");

            migrationBuilder.DropForeignKey(
                name: "FK_SelectionResults_AspNetUsers_AddedById",
                table: "SelectionResults");

            migrationBuilder.DropForeignKey(
                name: "FK_SelectionResults_AspNetUsers_DeletedById",
                table: "SelectionResults");

            migrationBuilder.DropIndex(
                name: "IX_SelectionResults_AddedById",
                table: "SelectionResults");

            migrationBuilder.DropIndex(
                name: "IX_SelectionResults_DeletedById",
                table: "SelectionResults");

            migrationBuilder.DropIndex(
                name: "IX_Schools_AddedById",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Schools_DeletedById",
                table: "Schools");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_AddedById",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_DeletedById",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_Criteria_AddedById",
                table: "Criteria");

            migrationBuilder.DropIndex(
                name: "IX_Criteria_DeletedById",
                table: "Criteria");

            migrationBuilder.DropIndex(
                name: "IX_CollegeSelections_AddedById",
                table: "CollegeSelections");

            migrationBuilder.DropIndex(
                name: "IX_CollegeSelections_DeletedById",
                table: "CollegeSelections");

            migrationBuilder.DropColumn(
                name: "AddedById",
                table: "SelectionResults");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "SelectionResults");

            migrationBuilder.DropColumn(
                name: "AddedById",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "AddedById",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "AddedById",
                table: "Criteria");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "Criteria");

            migrationBuilder.DropColumn(
                name: "AddedById",
                table: "CollegeSelections");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "CollegeSelections");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FreeUsed",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Introduction",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ReferralCode",
                table: "AspNetUsers");
        }
    }
}
