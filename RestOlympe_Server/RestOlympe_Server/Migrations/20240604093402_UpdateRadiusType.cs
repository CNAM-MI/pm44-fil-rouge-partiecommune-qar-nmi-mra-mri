using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestOlympe_Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRadiusType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "VoteRadiusKm",
                table: "lobbies",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "VoteRadiusKm",
                table: "lobbies",
                type: "real",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
