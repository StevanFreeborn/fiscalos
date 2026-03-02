using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiscalOS.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionsDbSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Account_AccountId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Account_AccountId1",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Users_UserId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionMetadata_Transaction_TransactionId",
                table: "TransactionMetadata");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction");

            migrationBuilder.RenameTable(
                name: "Transaction",
                newName: "Transactions");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_UserId",
                table: "Transactions",
                newName: "IX_Transactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_AccountId1",
                table: "Transactions",
                newName: "IX_Transactions_AccountId1");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_AccountId",
                table: "Transactions",
                newName: "IX_Transactions_AccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionMetadata_Transactions_TransactionId",
                table: "TransactionMetadata",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Account_AccountId",
                table: "Transactions",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Account_AccountId1",
                table: "Transactions",
                column: "AccountId1",
                principalTable: "Account",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionMetadata_Transactions_TransactionId",
                table: "TransactionMetadata");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Account_AccountId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Account_AccountId1",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "Transaction");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_UserId",
                table: "Transaction",
                newName: "IX_Transaction_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_AccountId1",
                table: "Transaction",
                newName: "IX_Transaction_AccountId1");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_AccountId",
                table: "Transaction",
                newName: "IX_Transaction_AccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Account_AccountId",
                table: "Transaction",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Account_AccountId1",
                table: "Transaction",
                column: "AccountId1",
                principalTable: "Account",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Users_UserId",
                table: "Transaction",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionMetadata_Transaction_TransactionId",
                table: "TransactionMetadata",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
