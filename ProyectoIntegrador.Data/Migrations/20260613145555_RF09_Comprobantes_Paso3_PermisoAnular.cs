using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoIntegrador.Data.Migrations
{
    /// <inheritdoc />
    public partial class RF09_Comprobantes_Paso3_PermisoAnular : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permisos",
                columns: new[] { "Id", "Accion", "Modulo", "Nombre" },
                values: new object[] { new Guid("b0000000-0005-0001-0001-000000000004"), "Anular", "Comprobantes", "Anular Comprobantes" });

            migrationBuilder.InsertData(
                table: "RolPermisos",
                columns: new[] { "PermisoId", "RolId" },
                values: new object[] { new Guid("b0000000-0005-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0005-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") });

            migrationBuilder.DeleteData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: new Guid("b0000000-0005-0001-0001-000000000004"));
        }
    }
}
