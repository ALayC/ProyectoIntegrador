using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoIntegrador.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddResumenResultadosCuenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CuentasContables",
                columns: new[] { "Id", "Codigo", "CuentaPadreId", "EsImputable", "EsSistema", "Estado", "Naturaleza", "Nombre", "PlanCuentasId", "Tipo" },
                values: new object[] { new Guid("d0000000-3200-0300-0000-000000000001"), "3.2.3", new Guid("d0000000-3200-0000-0000-000000000001"), true, true, "Activa", "Acreedora", "Resumen de resultados", new Guid("d0000000-0001-0001-0001-000000000001"), "Patrimonio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-3200-0300-0000-000000000001"));
        }
    }
}
