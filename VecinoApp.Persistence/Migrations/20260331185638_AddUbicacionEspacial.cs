using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace VecinoApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUbicacionEspacial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Point>(
                name: "Ubicacion",
                table: "Negocios",
                type: "geography",
                nullable: true);
            // ÍNDICE ESPACIAL (corregido)
            migrationBuilder.Sql(
                @"CREATE SPATIAL INDEX [IX_Negocios_Ubicacion] 
          ON [Negocios] ([Ubicacion])
          USING GEOGRAPHY_AUTO_GRID;");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Negocios_Ubicacion",
            //    table: "Negocios",
            //    column: "Ubicacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Negocios_Ubicacion",
                table: "Negocios");

            migrationBuilder.DropColumn(
                name: "Ubicacion",
                table: "Negocios");
        }
    }
}
