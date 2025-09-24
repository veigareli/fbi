using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Assists",
                table: "PlayerRoundPoints",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Blocks",
                table: "PlayerRoundPoints",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FantasyPoints",
                table: "PlayerRoundPoints",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Rebounds",
                table: "PlayerRoundPoints",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Steals",
                table: "PlayerRoundPoints",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TeamWin",
                table: "PlayerRoundPoints",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Turnovers",
                table: "PlayerRoundPoints",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CurrentRound",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoundNumber = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentRound", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrentRound");

            migrationBuilder.DropColumn(
                name: "Assists",
                table: "PlayerRoundPoints");

            migrationBuilder.DropColumn(
                name: "Blocks",
                table: "PlayerRoundPoints");

            migrationBuilder.DropColumn(
                name: "FantasyPoints",
                table: "PlayerRoundPoints");

            migrationBuilder.DropColumn(
                name: "Rebounds",
                table: "PlayerRoundPoints");

            migrationBuilder.DropColumn(
                name: "Steals",
                table: "PlayerRoundPoints");

            migrationBuilder.DropColumn(
                name: "TeamWin",
                table: "PlayerRoundPoints");

            migrationBuilder.DropColumn(
                name: "Turnovers",
                table: "PlayerRoundPoints");
        }
    }
}
