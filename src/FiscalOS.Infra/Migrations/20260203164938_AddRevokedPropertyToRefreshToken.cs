using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalOS.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddRevokedPropertyToRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Revoked",
                table: "RefreshTokens",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Revoked",
                table: "RefreshTokens");
        }
    }
}
