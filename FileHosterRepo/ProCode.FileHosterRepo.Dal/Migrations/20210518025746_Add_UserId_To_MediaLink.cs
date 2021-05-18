using Microsoft.EntityFrameworkCore.Migrations;

namespace ProCode.FileHosterRepo.Dal.Migrations
{
    public partial class Add_UserId_To_MediaLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaLinks_Users_UserId",
                table: "MediaLinks");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "MediaLinks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaLinks_Users_UserId",
                table: "MediaLinks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaLinks_Users_UserId",
                table: "MediaLinks");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "MediaLinks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaLinks_Users_UserId",
                table: "MediaLinks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
