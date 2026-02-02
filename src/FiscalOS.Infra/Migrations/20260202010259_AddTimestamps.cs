using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalOS.Infra.Migrations
{
  /// <inheritdoc />
  public partial class AddTimestamps : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "CreatedAt",
          table: "Users",
          type: "TEXT",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

      migrationBuilder.AddColumn<string>(
          name: "HashedPassword",
          table: "Users",
          type: "TEXT",
          nullable: false,
          defaultValue: "");

      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "UpdatedAt",
          table: "Users",
          type: "TEXT",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "CreatedAt",
          table: "RefreshTokens",
          type: "TEXT",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

      migrationBuilder.AddColumn<DateTimeOffset>(
          name: "UpdatedAt",
          table: "RefreshTokens",
          type: "TEXT",
          nullable: false,
          defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "CreatedAt",
          table: "Users");

      migrationBuilder.DropColumn(
          name: "HashedPassword",
          table: "Users");

      migrationBuilder.DropColumn(
          name: "UpdatedAt",
          table: "Users");

      migrationBuilder.DropColumn(
          name: "CreatedAt",
          table: "RefreshTokens");

      migrationBuilder.DropColumn(
          name: "UpdatedAt",
          table: "RefreshTokens");
    }
  }
}