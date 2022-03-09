using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceLite.Api.Data.Migrations
{
    public partial class ScopeInvoiceNumbersToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_Number",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_UserId",
                table: "Invoices");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_UserId_Number",
                table: "Invoices",
                columns: new[] { "UserId", "Number" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_UserId_Number",
                table: "Invoices");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Number",
                table: "Invoices",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_UserId",
                table: "Invoices",
                column: "UserId");
        }
    }
}
