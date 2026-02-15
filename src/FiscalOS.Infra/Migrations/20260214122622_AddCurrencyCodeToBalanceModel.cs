using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalOS.Infra.Migrations
{
  /// <inheritdoc />
  public partial class AddCurrencyCodeToBalanceModel : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
          name: "CurrencyCode",
          table: "Balance",
          type: "TEXT",
          nullable: false,
          defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "CurrencyCode",
          table: "Balance");
    }
  }
}