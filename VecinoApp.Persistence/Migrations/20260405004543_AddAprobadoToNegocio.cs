using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VecinoApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAprobadoToNegocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Aprobado",
                table: "Negocios",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aprobado",
                table: "Negocios");
        }
    }
}
