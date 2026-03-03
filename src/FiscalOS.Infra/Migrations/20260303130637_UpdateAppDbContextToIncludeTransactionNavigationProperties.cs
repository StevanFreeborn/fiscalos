using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalOS.Infra.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppDbContextToIncludeTransactionNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Account_AccountId1",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_UserId1",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_AccountId1",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId1",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "AccountId1",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Transactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AccountId1",
                table: "Transactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Transactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AccountId1",
                table: "Transactions",
                column: "AccountId1");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId1",
                table: "Transactions",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Account_AccountId1",
                table: "Transactions",
                column: "AccountId1",
                principalTable: "Account",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_UserId1",
                table: "Transactions",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
