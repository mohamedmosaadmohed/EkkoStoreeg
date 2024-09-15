using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EkkoSoreeg.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class fff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "TbOrderHeaders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "shippingCost",
                table: "TbOrderHeaders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "TbOrderHeaders");

            migrationBuilder.DropColumn(
                name: "shippingCost",
                table: "TbOrderHeaders");
        }
    }
}
