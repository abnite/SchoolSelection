using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSelection.Migrations
{
    public partial class createView_collegeFitHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollegeFitHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollegeSelectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollegeFitHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollegeFitHistories_AspNetUsers_AddedById",
                        column: x => x.AddedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollegeFitHistories_AspNetUsers_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollegeFitHistories_CollegeSelections_CollegeSelectionId",
                        column: x => x.CollegeSelectionId,
                        principalTable: "CollegeSelections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollegeFitHistories_AddedById",
                table: "CollegeFitHistories",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeFitHistories_CollegeSelectionId",
                table: "CollegeFitHistories",
                column: "CollegeSelectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeFitHistories_DeletedById",
                table: "CollegeFitHistories",
                column: "DeletedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollegeFitHistories");
        }
    }
}
