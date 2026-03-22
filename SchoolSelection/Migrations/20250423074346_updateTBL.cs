using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolSelection.Migrations
{
    public partial class updateTBL : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CollegeCriteriaSummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollegeSelectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriteriaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastChecked = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ManuallyUpdated = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollegeCriteriaSummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollegeCriteriaSummaries_AspNetUsers_AddedById",
                        column: x => x.AddedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollegeCriteriaSummaries_AspNetUsers_DeletedById",
                        column: x => x.DeletedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollegeCriteriaSummaries_CollegeSelections_CollegeSelectionId",
                        column: x => x.CollegeSelectionId,
                        principalTable: "CollegeSelections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollegeCriteriaSummaries_Criteria_CriteriaId",
                        column: x => x.CriteriaId,
                        principalTable: "Criteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CollegeCriteriaSummaries_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollegeCriteriaSummaries_AddedById",
                table: "CollegeCriteriaSummaries",
                column: "AddedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeCriteriaSummaries_CollegeSelectionId",
                table: "CollegeCriteriaSummaries",
                column: "CollegeSelectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeCriteriaSummaries_CriteriaId",
                table: "CollegeCriteriaSummaries",
                column: "CriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeCriteriaSummaries_DeletedById",
                table: "CollegeCriteriaSummaries",
                column: "DeletedById");

            migrationBuilder.CreateIndex(
                name: "IX_CollegeCriteriaSummaries_SchoolId",
                table: "CollegeCriteriaSummaries",
                column: "SchoolId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollegeCriteriaSummaries");
        }
    }
}
