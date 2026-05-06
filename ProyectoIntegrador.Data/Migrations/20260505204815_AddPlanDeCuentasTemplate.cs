using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProyectoIntegrador.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanDeCuentasTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlanesDeCuentas_ClienteId",
                table: "PlanesDeCuentas");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClienteId",
                table: "PlanesDeCuentas",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<bool>(
                name: "EsTemplate",
                table: "PlanesDeCuentas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "PlanesDeCuentas",
                columns: new[] { "Id", "ClienteId", "EsTemplate" },
                values: new object[] { new Guid("d0000000-0001-0001-0001-000000000001"), null, true });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0001-0001-0001-000000000001"),
                column: "PasswordHash",
                value: "$2a$12$siCyK43j/60igAgW0GwTXOojpsf5pt0X6IIu9I5FfBhE645FlNcLW");

            migrationBuilder.InsertData(
                table: "CuentasContables",
                columns: new[] { "Id", "Codigo", "CuentaPadreId", "EsImputable", "Estado", "Naturaleza", "Nombre", "PlanCuentasId", "Tipo" },
                values: new object[,]
                {
                    { new Guid("d0000000-1000-0000-0000-000000000001"), "1", null, false, "Activa", "Deudora", "Activo", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-2000-0000-0000-000000000001"), "2", null, false, "Activa", "Acreedora", "Pasivo", new Guid("d0000000-0001-0001-0001-000000000001"), "Pasivo" },
                    { new Guid("d0000000-1100-0000-0000-000000000001"), "1.1", new Guid("d0000000-1000-0000-0000-000000000001"), true, "Activa", "Deudora", "Caja", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-1200-0000-0000-000000000001"), "1.2", new Guid("d0000000-1000-0000-0000-000000000001"), true, "Activa", "Deudora", "Bancos", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanesDeCuentas_ClienteId",
                table: "PlanesDeCuentas",
                column: "ClienteId",
                unique: true,
                filter: "[ClienteId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlanesDeCuentas_ClienteId",
                table: "PlanesDeCuentas");

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1100-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1200-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "PlanesDeCuentas",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0001-0001-0001-000000000001"));

            migrationBuilder.DropColumn(
                name: "EsTemplate",
                table: "PlanesDeCuentas");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClienteId",
                table: "PlanesDeCuentas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0001-0001-0001-000000000001"),
                column: "PasswordHash",
                value: "$2a$12$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi");

            migrationBuilder.CreateIndex(
                name: "IX_PlanesDeCuentas_ClienteId",
                table: "PlanesDeCuentas",
                column: "ClienteId",
                unique: true);
        }
    }
}
