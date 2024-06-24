using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class PlayerInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Hp",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MapId",
                table: "Player",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "MapId",
                table: "Player");
        }
    }
}
