using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class Item : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Player_PlayerName",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "PlayerName",
                table: "Player");

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    ItemInfoDbId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    PlayerDbId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.ItemInfoDbId);
                    table.ForeignKey(
                        name: "FK_Item_Player_PlayerDbId",
                        column: x => x.PlayerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Player_PlayerDbId",
                table: "Player",
                column: "PlayerDbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameAccount_GameAccountDbId",
                table: "GameAccount",
                column: "GameAccountDbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_ItemInfoDbId",
                table: "Item",
                column: "ItemInfoDbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_PlayerDbId",
                table: "Item",
                column: "PlayerDbId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Player_PlayerDbId",
                table: "Player");

            migrationBuilder.DropIndex(
                name: "IX_GameAccount_GameAccountDbId",
                table: "GameAccount");

            migrationBuilder.AddColumn<string>(
                name: "PlayerName",
                table: "Player",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Player_PlayerName",
                table: "Player",
                column: "PlayerName",
                unique: true,
                filter: "[PlayerName] IS NOT NULL");
        }
    }
}
