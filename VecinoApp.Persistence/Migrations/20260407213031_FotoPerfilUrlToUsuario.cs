using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VecinoApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FotoPerfilUrlToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoPerfilUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoPerfilUrl",
                table: "AspNetUsers");
        }
    }
}
