using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prototype.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderIssuedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "IssuedDate",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IssuedDate",
                table: "Orders");
        }
    }
}
