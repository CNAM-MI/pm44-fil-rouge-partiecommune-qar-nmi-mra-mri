using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestOlympe_Server.Migrations
{
    /// <inheritdoc />
    public partial class AddLongitudeLatitudeToLobby : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "lobbies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "lobbies",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "lobbies");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "lobbies");
        }
    }
}
