using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VecinoApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDescuentoToPromocion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Descuento",
                table: "Promociones",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descuento",
                table: "Promociones");
        }
    }
}
