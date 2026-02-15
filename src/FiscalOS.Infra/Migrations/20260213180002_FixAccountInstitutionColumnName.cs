using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalOS.Infra.Migrations
{
  /// <inheritdoc />
  public partial class FixAccountInstitutionColumnName : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK_Account_Institution_InstituionId",
          table: "Account");

      migrationBuilder.RenameColumn(
          name: "InstituionId",
          table: "Account",
          newName: "InstitutionId");

      migrationBuilder.RenameIndex(
          name: "IX_Account_InstituionId",
          table: "Account",
          newName: "IX_Account_InstitutionId");

      migrationBuilder.AddForeignKey(
          name: "FK_Account_Institution_InstitutionId",
          table: "Account",
          column: "InstitutionId",
          principalTable: "Institution",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK_Account_Institution_InstitutionId",
          table: "Account");

      migrationBuilder.RenameColumn(
          name: "InstitutionId",
          table: "Account",
          newName: "InstituionId");

      migrationBuilder.RenameIndex(
          name: "IX_Account_InstitutionId",
          table: "Account",
          newName: "IX_Account_InstituionId");

      migrationBuilder.AddForeignKey(
          name: "FK_Account_Institution_InstituionId",
          table: "Account",
          column: "InstituionId",
          principalTable: "Institution",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);
    }
  }
}