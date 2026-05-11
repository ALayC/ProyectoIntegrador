using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoIntegrador.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDateTimeToDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TiposDeCambio_Moneda_Fecha",
                table: "TiposDeCambio");

            migrationBuilder.DropIndex(
                name: "IX_AsientosContables_ClienteId_EjercicioId_Fecha",
                table: "AsientosContables");

            migrationBuilder.DropIndex(
                name: "IX_SaldosCuenta_ClienteId_CuentaContableId_Periodo",
                table: "SaldosCuenta");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Fecha",
                table: "TiposDeCambio",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Periodo",
                table: "SaldosCuenta",
                type: "date",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "FechaInicio",
                table: "EjerciciosContables",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "FechaFin",
                table: "EjerciciosContables",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Fecha",
                table: "AsientosContables",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_TiposDeCambio_Moneda_Fecha",
                table: "TiposDeCambio",
                columns: new[] { "Moneda", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_ClienteId_EjercicioId_Fecha",
                table: "AsientosContables",
                columns: new[] { "ClienteId", "EjercicioId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_SaldosCuenta_ClienteId_CuentaContableId_Periodo",
                table: "SaldosCuenta",
                columns: new[] { "ClienteId", "CuentaContableId", "Periodo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TiposDeCambio_Moneda_Fecha",
                table: "TiposDeCambio");

            migrationBuilder.DropIndex(
                name: "IX_AsientosContables_ClienteId_EjercicioId_Fecha",
                table: "AsientosContables");

            migrationBuilder.DropIndex(
                name: "IX_SaldosCuenta_ClienteId_CuentaContableId_Periodo",
                table: "SaldosCuenta");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "TiposDeCambio",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "Periodo",
                table: "SaldosCuenta",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaInicio",
                table: "EjerciciosContables",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaFin",
                table: "EjerciciosContables",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Fecha",
                table: "AsientosContables",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.CreateIndex(
                name: "IX_TiposDeCambio_Moneda_Fecha",
                table: "TiposDeCambio",
                columns: new[] { "Moneda", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_ClienteId_EjercicioId_Fecha",
                table: "AsientosContables",
                columns: new[] { "ClienteId", "EjercicioId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_SaldosCuenta_ClienteId_CuentaContableId_Periodo",
                table: "SaldosCuenta",
                columns: new[] { "ClienteId", "CuentaContableId", "Periodo" });
        }
    }
}
