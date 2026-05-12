using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoIntegrador.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEsSistemaACuentasContables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsSistema",
                table: "CuentasContables",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1000-0000-0000-000000000001"),
                column: "EsSistema",
                value: true);

            migrationBuilder.UpdateData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1100-0000-0000-000000000001"),
                column: "EsSistema",
                value: false);

            migrationBuilder.UpdateData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1200-0000-0000-000000000001"),
                column: "EsSistema",
                value: false);

            migrationBuilder.UpdateData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2000-0000-0000-000000000001"),
                column: "EsSistema",
                value: true);

            migrationBuilder.UpdateData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-3000-0000-0000-000000000001"),
                column: "EsSistema",
                value: true);

            migrationBuilder.UpdateData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-4000-0000-0000-000000000001"),
                column: "EsSistema",
                value: true);

            migrationBuilder.UpdateData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5000-0000-0000-000000000001"),
                column: "EsSistema",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsSistema",
                table: "CuentasContables");
        }
    }
}
