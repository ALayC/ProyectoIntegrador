using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProyectoIntegrador.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlanCuentasTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CuentasContables",
                columns: new[] { "Id", "Codigo", "CuentaPadreId", "EsImputable", "Estado", "Naturaleza", "Nombre", "PlanCuentasId", "Tipo" },
                values: new object[,]
                {
                    { new Guid("d0000000-3000-0000-0000-000000000001"), "3", null, false, "Activa", "Acreedora", "Patrimonio", new Guid("d0000000-0001-0001-0001-000000000001"), "Patrimonio" },
                    { new Guid("d0000000-4000-0000-0000-000000000001"), "4", null, false, "Activa", "Acreedora", "Ingresos", new Guid("d0000000-0001-0001-0001-000000000001"), "Ingreso" },
                    { new Guid("d0000000-5000-0000-0000-000000000001"), "5", null, false, "Activa", "Deudora", "Egresos", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-3000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-4000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5000-0000-0000-000000000001"));
        }
    }
}
