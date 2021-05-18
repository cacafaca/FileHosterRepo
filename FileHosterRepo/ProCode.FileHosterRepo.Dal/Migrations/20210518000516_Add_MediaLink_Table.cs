using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace ProCode.FileHosterRepo.Dal.Migrations
{
    public partial class Add_MediaLink_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaLinks",
                columns: table => new
                {
                    MediaLinkId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MediaPartId = table.Column<int>(type: "int", nullable: false),
                    VersionId = table.Column<int>(type: "int", nullable: false),
                    LinkId = table.Column<int>(type: "int", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    VersionComment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaLinks", x => x.MediaLinkId);
                    table.ForeignKey(
                        name: "FK_MediaLinks_MediaParts_MediaPartId",
                        column: x => x.MediaPartId,
                        principalTable: "MediaParts",
                        principalColumn: "MediaPartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediaLinks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaLinks_MediaPartId_VersionId_LinkId",
                table: "MediaLinks",
                columns: new[] { "MediaPartId", "VersionId", "LinkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaLinks_UserId",
                table: "MediaLinks",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaLinks");
        }
    }
}
