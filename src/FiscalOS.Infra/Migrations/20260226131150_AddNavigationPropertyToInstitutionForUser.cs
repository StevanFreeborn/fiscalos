using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalOS.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationPropertyToInstitutionForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Institution_InstitutionId",
                table: "Account");

            migrationBuilder.DropForeignKey(
                name: "FK_Institution_Users_UserId",
                table: "Institution");

            migrationBuilder.DropForeignKey(
                name: "FK_InstitutionMetadata_Institution_InstitutionId",
                table: "InstitutionMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Institution",
                table: "Institution");

            migrationBuilder.RenameTable(
                name: "Institution",
                newName: "Institutions");

            migrationBuilder.RenameIndex(
                name: "IX_Institution_UserId",
                table: "Institutions",
                newName: "IX_Institutions_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Institutions",
                table: "Institutions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Account_Institutions_InstitutionId",
                table: "Account",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstitutionMetadata_Institutions_InstitutionId",
                table: "InstitutionMetadata",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Institutions_Users_UserId",
                table: "Institutions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Institutions_InstitutionId",
                table: "Account");

            migrationBuilder.DropForeignKey(
                name: "FK_InstitutionMetadata_Institutions_InstitutionId",
                table: "InstitutionMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_Institutions_Users_UserId",
                table: "Institutions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Institutions",
                table: "Institutions");

            migrationBuilder.RenameTable(
                name: "Institutions",
                newName: "Institution");

            migrationBuilder.RenameIndex(
                name: "IX_Institutions_UserId",
                table: "Institution",
                newName: "IX_Institution_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Institution",
                table: "Institution",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Account_Institution_InstitutionId",
                table: "Account",
                column: "InstitutionId",
                principalTable: "Institution",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Institution_Users_UserId",
                table: "Institution",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstitutionMetadata_Institution_InstitutionId",
                table: "InstitutionMetadata",
                column: "InstitutionId",
                principalTable: "Institution",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
