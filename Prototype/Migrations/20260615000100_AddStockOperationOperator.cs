using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prototype.Migrations
{
    /// <inheritdoc />
    public partial class AddStockOperationOperator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OperatorLogin",
                table: "StockOperations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OperatorLogin",
                table: "StockOperations");
        }
    }
}
