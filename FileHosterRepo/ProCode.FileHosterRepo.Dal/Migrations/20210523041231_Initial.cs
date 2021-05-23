using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace ProCode.FileHosterRepo.Dal.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media_tag",
                columns: table => new
                {
                    MediaTagId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true, collation: "SQL_Latin1_General_CP1_CI_AS")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_tag", x => x.MediaTagId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Password = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Nickname = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Logged = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "media_header",
                columns: table => new
                {
                    MediaHeaderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ReferenceLink = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_header", x => x.MediaHeaderId);
                    table.ForeignKey(
                        name: "FK_media_header_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_header_tag",
                columns: table => new
                {
                    MediaHeaderId = table.Column<int>(type: "int", nullable: false),
                    MediaTagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_header_tag", x => new { x.MediaHeaderId, x.MediaTagId });
                    table.ForeignKey(
                        name: "FK_media_header_tag_media_header_MediaHeaderId",
                        column: x => x.MediaHeaderId,
                        principalTable: "media_header",
                        principalColumn: "MediaHeaderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_media_header_tag_media_tag_MediaTagId",
                        column: x => x.MediaTagId,
                        principalTable: "media_tag",
                        principalColumn: "MediaTagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_part",
                columns: table => new
                {
                    MediaPartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MediaHeaderId = table.Column<int>(type: "int", nullable: false),
                    Season = table.Column<int>(type: "int", nullable: false),
                    Episode = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    ReferenceLink = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_part", x => x.MediaPartId);
                    table.ForeignKey(
                        name: "FK_media_part_media_header_MediaHeaderId",
                        column: x => x.MediaHeaderId,
                        principalTable: "media_header",
                        principalColumn: "MediaHeaderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_media_part_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_part_tag",
                columns: table => new
                {
                    MediaPartId = table.Column<int>(type: "int", nullable: false),
                    MediaTagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_part_tag", x => new { x.MediaPartId, x.MediaTagId });
                    table.ForeignKey(
                        name: "FK_media_part_tag_media_part_MediaPartId",
                        column: x => x.MediaPartId,
                        principalTable: "media_part",
                        principalColumn: "MediaPartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_media_part_tag_media_tag_MediaTagId",
                        column: x => x.MediaTagId,
                        principalTable: "media_tag",
                        principalColumn: "MediaTagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_version",
                columns: table => new
                {
                    MediaVersionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MediaPartId = table.Column<int>(type: "int", nullable: false),
                    VersionComment = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_version", x => x.MediaVersionId);
                    table.ForeignKey(
                        name: "FK_media_version_media_part_MediaPartId",
                        column: x => x.MediaPartId,
                        principalTable: "media_part",
                        principalColumn: "MediaPartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_media_version_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_link",
                columns: table => new
                {
                    MediaLinkId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MediaVersionId = table.Column<int>(type: "int", nullable: false),
                    LinkOrderId = table.Column<int>(type: "int", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VersionComment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_link", x => x.MediaLinkId);
                    table.ForeignKey(
                        name: "FK_media_link_media_version_MediaVersionId",
                        column: x => x.MediaVersionId,
                        principalTable: "media_version",
                        principalColumn: "MediaVersionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_media_link_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "media_version_tag",
                columns: table => new
                {
                    MediaVersionId = table.Column<int>(type: "int", nullable: false),
                    MediaTagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_version_tag", x => new { x.MediaVersionId, x.MediaTagId });
                    table.ForeignKey(
                        name: "FK_media_version_tag_media_tag_MediaTagId",
                        column: x => x.MediaTagId,
                        principalTable: "media_tag",
                        principalColumn: "MediaTagId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_media_version_tag_media_version_MediaVersionId",
                        column: x => x.MediaVersionId,
                        principalTable: "media_version",
                        principalColumn: "MediaVersionId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Connection between MediaVersions and MediaTags tables.");

            migrationBuilder.CreateIndex(
                name: "IX_media_header_UserId",
                table: "media_header",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_media_header_tag_MediaTagId",
                table: "media_header_tag",
                column: "MediaTagId");

            migrationBuilder.CreateIndex(
                name: "IX_media_link_MediaVersionId_LinkOrderId",
                table: "media_link",
                columns: new[] { "MediaVersionId", "LinkOrderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_link_UserId",
                table: "media_link",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_media_part_MediaHeaderId_Season_Episode",
                table: "media_part",
                columns: new[] { "MediaHeaderId", "Season", "Episode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_part_UserId",
                table: "media_part",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_media_part_tag_MediaTagId",
                table: "media_part_tag",
                column: "MediaTagId");

            migrationBuilder.CreateIndex(
                name: "IX_media_tag_Name",
                table: "media_tag",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_version_MediaPartId",
                table: "media_version",
                column: "MediaPartId");

            migrationBuilder.CreateIndex(
                name: "IX_media_version_UserId",
                table: "media_version",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_media_version_tag_MediaTagId",
                table: "media_version_tag",
                column: "MediaTagId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Nickname",
                table: "Users",
                column: "Nickname",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media_header_tag");

            migrationBuilder.DropTable(
                name: "media_link");

            migrationBuilder.DropTable(
                name: "media_part_tag");

            migrationBuilder.DropTable(
                name: "media_version_tag");

            migrationBuilder.DropTable(
                name: "media_tag");

            migrationBuilder.DropTable(
                name: "media_version");

            migrationBuilder.DropTable(
                name: "media_part");

            migrationBuilder.DropTable(
                name: "media_header");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
