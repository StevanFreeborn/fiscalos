using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalOS.Infra.Migrations
{
  /// <inheritdoc />
  public partial class AddUsernameToUsers : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
          name: "Username",
          table: "Users",
          type: "TEXT",
          nullable: false,
          defaultValue: "");

      migrationBuilder.AddColumn<Guid>(
          name: "UserId",
          table: "RefreshTokens",
          type: "TEXT",
          nullable: false,
          defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

      migrationBuilder.CreateIndex(
          name: "IX_RefreshTokens_UserId",
          table: "RefreshTokens",
          column: "UserId");

      migrationBuilder.AddForeignKey(
          name: "FK_RefreshTokens_Users_UserId",
          table: "RefreshTokens",
          column: "UserId",
          principalTable: "Users",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK_RefreshTokens_Users_UserId",
          table: "RefreshTokens");

      migrationBuilder.DropIndex(
          name: "IX_RefreshTokens_UserId",
          table: "RefreshTokens");

      migrationBuilder.DropColumn(
          name: "Username",
          table: "Users");

      migrationBuilder.DropColumn(
          name: "UserId",
          table: "RefreshTokens");
    }
  }
}