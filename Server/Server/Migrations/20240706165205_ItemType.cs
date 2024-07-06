using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class ItemType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "Type",
                table: "Item",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Item",
                type: "int",
                nullable: false,
                oldClrType: typeof(byte));
        }
    }
}
