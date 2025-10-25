using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication2.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentStatusToStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasPaid",
                table: "Stores");

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "Stores",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Stores");

            migrationBuilder.AddColumn<bool>(
                name: "HasPaid",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
