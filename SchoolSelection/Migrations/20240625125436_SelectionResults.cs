using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSelection.Migrations
{
    public partial class SelectionResults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SelectionResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollegeSelectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SelectionResults_CollegeSelections_CollegeSelectionId",
                        column: x => x.CollegeSelectionId,
                        principalTable: "CollegeSelections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SelectionResults_CollegeSelectionId",
                table: "SelectionResults",
                column: "CollegeSelectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SelectionResults");
        }
    }
}
