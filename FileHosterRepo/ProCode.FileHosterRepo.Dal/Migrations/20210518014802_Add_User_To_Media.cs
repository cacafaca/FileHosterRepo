using Microsoft.EntityFrameworkCore.Migrations;

namespace ProCode.FileHosterRepo.Dal.Migrations
{
    public partial class Add_User_To_Media : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Medias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Medias_UserId",
                table: "Medias",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_Users_UserId",
                table: "Medias",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medias_Users_UserId",
                table: "Medias");

            migrationBuilder.DropIndex(
                name: "IX_Medias_UserId",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Medias");
        }
    }
}
