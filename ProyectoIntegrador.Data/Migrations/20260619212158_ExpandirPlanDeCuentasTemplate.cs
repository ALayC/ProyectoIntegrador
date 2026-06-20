using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProyectoIntegrador.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExpandirPlanDeCuentasTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1100-0000-0000-000000000001"),
                columns: new[] { "EsImputable", "Nombre" },
                values: new object[] { false, "Activo Corriente" });

            migrationBuilder.UpdateData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1200-0000-0000-000000000001"),
                columns: new[] { "EsImputable", "Nombre" },
                values: new object[] { false, "Activo No Corriente" });

            migrationBuilder.InsertData(
                table: "CuentasContables",
                columns: new[] { "Id", "Codigo", "CuentaPadreId", "EsImputable", "EsSistema", "Estado", "Naturaleza", "Nombre", "PlanCuentasId", "Tipo" },
                values: new object[,]
                {
                    { new Guid("d0000000-1100-0100-0000-000000000001"), "1.1.1", new Guid("d0000000-1100-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Caja", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-1100-0200-0000-000000000001"), "1.1.2", new Guid("d0000000-1100-0000-0000-000000000001"), false, false, "Activa", "Deudora", "Bancos", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-1100-0300-0000-000000000001"), "1.1.3", new Guid("d0000000-1100-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Clientes a cobrar", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-1100-0400-0000-000000000001"), "1.1.4", new Guid("d0000000-1100-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Deudores varios", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-1100-0500-0000-000000000001"), "1.1.5", new Guid("d0000000-1100-0000-0000-000000000001"), true, false, "Activa", "Deudora", "IVA Crédito Fiscal", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-1100-0600-0000-000000000001"), "1.1.6", new Guid("d0000000-1100-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Anticipos a proveedores", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-1200-0100-0000-000000000001"), "1.2.1", new Guid("d0000000-1200-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Inmuebles", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-1200-0200-0000-000000000001"), "1.2.2", new Guid("d0000000-1200-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Muebles y útiles", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-1200-0300-0000-000000000001"), "1.2.3", new Guid("d0000000-1200-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Equipos de computación", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-1200-0400-0000-000000000001"), "1.2.4", new Guid("d0000000-1200-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Rodados", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-1200-0500-0000-000000000001"), "1.2.5", new Guid("d0000000-1200-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Depreciación acumulada (activos)", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-2100-0000-0000-000000000001"), "2.1", new Guid("d0000000-2000-0000-0000-000000000001"), false, false, "Activa", "Acreedora", "Pasivo Corriente", new Guid("d0000000-0001-0001-0001-000000000001"), "Pasivo" },
                    { new Guid("d0000000-2200-0000-0000-000000000001"), "2.2", new Guid("d0000000-2000-0000-0000-000000000001"), false, false, "Activa", "Acreedora", "Pasivo No Corriente", new Guid("d0000000-0001-0001-0001-000000000001"), "Pasivo" },
                    { new Guid("d0000000-3100-0000-0000-000000000001"), "3.1", new Guid("d0000000-3000-0000-0000-000000000001"), false, false, "Activa", "Acreedora", "Capital", new Guid("d0000000-0001-0001-0001-000000000001"), "Patrimonio" },
                    { new Guid("d0000000-3200-0000-0000-000000000001"), "3.2", new Guid("d0000000-3000-0000-0000-000000000001"), false, false, "Activa", "Acreedora", "Resultados", new Guid("d0000000-0001-0001-0001-000000000001"), "Patrimonio" },
                    { new Guid("d0000000-4100-0000-0000-000000000001"), "4.1", new Guid("d0000000-4000-0000-0000-000000000001"), false, false, "Activa", "Acreedora", "Ingresos Operativos", new Guid("d0000000-0001-0001-0001-000000000001"), "Ingreso" },
                    { new Guid("d0000000-4200-0000-0000-000000000001"), "4.2", new Guid("d0000000-4000-0000-0000-000000000001"), false, false, "Activa", "Acreedora", "Ingresos No Operativos", new Guid("d0000000-0001-0001-0001-000000000001"), "Ingreso" },
                    { new Guid("d0000000-5100-0000-0000-000000000001"), "5.1", new Guid("d0000000-5000-0000-0000-000000000001"), false, false, "Activa", "Deudora", "Costos", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5200-0000-0000-000000000001"), "5.2", new Guid("d0000000-5000-0000-0000-000000000001"), false, false, "Activa", "Deudora", "Gastos de Personal", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5300-0000-0000-000000000001"), "5.3", new Guid("d0000000-5000-0000-0000-000000000001"), false, false, "Activa", "Deudora", "Gastos Generales", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5400-0000-0000-000000000001"), "5.4", new Guid("d0000000-5000-0000-0000-000000000001"), false, false, "Activa", "Deudora", "Gastos Financieros", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5500-0000-0000-000000000001"), "5.5", new Guid("d0000000-5000-0000-0000-000000000001"), false, false, "Activa", "Deudora", "Depreciaciones", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5600-0000-0000-000000000001"), "5.6", new Guid("d0000000-5000-0000-0000-000000000001"), false, false, "Activa", "Deudora", "Impuestos y Contribuciones", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-1100-0200-0100-000000000001"), "1.1.2.1", new Guid("d0000000-1100-0200-0000-000000000001"), true, false, "Activa", "Deudora", "BROU", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-1100-0200-0200-000000000001"), "1.1.2.2", new Guid("d0000000-1100-0200-0000-000000000001"), true, false, "Activa", "Deudora", "Itaú", new Guid("d0000000-0001-0001-0001-000000000001"), "Activo" },
                    { new Guid("d0000000-2100-0100-0000-000000000001"), "2.1.1", new Guid("d0000000-2100-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Proveedores a pagar", new Guid("d0000000-0001-0001-0001-000000000001"), "Pasivo" },
                    { new Guid("d0000000-2100-0200-0000-000000000001"), "2.1.2", new Guid("d0000000-2100-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Acreedores varios", new Guid("d0000000-0001-0001-0001-000000000001"), "Pasivo" },
                    { new Guid("d0000000-2100-0300-0000-000000000001"), "2.1.3", new Guid("d0000000-2100-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "IVA Débito Fiscal", new Guid("d0000000-0001-0001-0001-000000000001"), "Pasivo" },
                    { new Guid("d0000000-2100-0400-0000-000000000001"), "2.1.4", new Guid("d0000000-2100-0000-0000-000000000001"), false, false, "Activa", "Acreedora", "Retenciones a pagar", new Guid("d0000000-0001-0001-0001-000000000001"), "Pasivo" },
                    { new Guid("d0000000-2100-0500-0000-000000000001"), "2.1.5", new Guid("d0000000-2100-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Sueldos a pagar", new Guid("d0000000-0001-0001-0001-000000000001"), "Pasivo" },
                    { new Guid("d0000000-2100-0600-0000-000000000001"), "2.1.6", new Guid("d0000000-2100-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Anticipos de clientes", new Guid("d0000000-0001-0001-0001-000000000001"), "Pasivo" },
                    { new Guid("d0000000-2200-0100-0000-000000000001"), "2.2.1", new Guid("d0000000-2200-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Préstamos bancarios LP", new Guid("d0000000-0001-0001-0001-000000000001"), "Pasivo" },
                    { new Guid("d0000000-2200-0200-0000-000000000001"), "2.2.2", new Guid("d0000000-2200-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Otras deudas LP", new Guid("d0000000-0001-0001-0001-000000000001"), "Pasivo" },
                    { new Guid("d0000000-3100-0100-0000-000000000001"), "3.1.1", new Guid("d0000000-3100-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Capital social", new Guid("d0000000-0001-0001-0001-000000000001"), "Patrimonio" },
                    { new Guid("d0000000-3100-0200-0000-000000000001"), "3.1.2", new Guid("d0000000-3100-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Aportes irrevocables", new Guid("d0000000-0001-0001-0001-000000000001"), "Patrimonio" },
                    { new Guid("d0000000-3200-0100-0000-000000000001"), "3.2.1", new Guid("d0000000-3200-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Resultados acumulados", new Guid("d0000000-0001-0001-0001-000000000001"), "Patrimonio" },
                    { new Guid("d0000000-3200-0200-0000-000000000001"), "3.2.2", new Guid("d0000000-3200-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Resultado del ejercicio", new Guid("d0000000-0001-0001-0001-000000000001"), "Patrimonio" },
                    { new Guid("d0000000-4100-0100-0000-000000000001"), "4.1.1", new Guid("d0000000-4100-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Ventas de mercadería", new Guid("d0000000-0001-0001-0001-000000000001"), "Ingreso" },
                    { new Guid("d0000000-4100-0200-0000-000000000001"), "4.1.2", new Guid("d0000000-4100-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Ventas de servicios", new Guid("d0000000-0001-0001-0001-000000000001"), "Ingreso" },
                    { new Guid("d0000000-4100-0300-0000-000000000001"), "4.1.3", new Guid("d0000000-4100-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Descuentos obtenidos", new Guid("d0000000-0001-0001-0001-000000000001"), "Ingreso" },
                    { new Guid("d0000000-4200-0100-0000-000000000001"), "4.2.1", new Guid("d0000000-4200-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Intereses ganados", new Guid("d0000000-0001-0001-0001-000000000001"), "Ingreso" },
                    { new Guid("d0000000-4200-0200-0000-000000000001"), "4.2.2", new Guid("d0000000-4200-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Diferencia de cambio ganada", new Guid("d0000000-0001-0001-0001-000000000001"), "Ingreso" },
                    { new Guid("d0000000-4200-0300-0000-000000000001"), "4.2.3", new Guid("d0000000-4200-0000-0000-000000000001"), true, false, "Activa", "Acreedora", "Otros ingresos", new Guid("d0000000-0001-0001-0001-000000000001"), "Ingreso" },
                    { new Guid("d0000000-5100-0100-0000-000000000001"), "5.1.1", new Guid("d0000000-5100-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Costo de mercadería vendida", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5200-0100-0000-000000000001"), "5.2.1", new Guid("d0000000-5200-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Sueldos y jornales", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5200-0200-0000-000000000001"), "5.2.2", new Guid("d0000000-5200-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Aportes patronales (BPS)", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5200-0300-0000-000000000001"), "5.2.3", new Guid("d0000000-5200-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Otros gastos de personal", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5300-0100-0000-000000000001"), "5.3.1", new Guid("d0000000-5300-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Alquiler", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5300-0200-0000-000000000001"), "5.3.2", new Guid("d0000000-5300-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Servicios (luz, agua, internet)", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5300-0300-0000-000000000001"), "5.3.3", new Guid("d0000000-5300-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Papelería y útiles de oficina", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5300-0400-0000-000000000001"), "5.3.4", new Guid("d0000000-5300-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Mantenimiento y reparaciones", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5300-0500-0000-000000000001"), "5.3.5", new Guid("d0000000-5300-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Seguros", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5400-0100-0000-000000000001"), "5.4.1", new Guid("d0000000-5400-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Intereses pagados", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5400-0200-0000-000000000001"), "5.4.2", new Guid("d0000000-5400-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Comisiones bancarias", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5400-0300-0000-000000000001"), "5.4.3", new Guid("d0000000-5400-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Diferencia de cambio perdida", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5500-0100-0000-000000000001"), "5.5.1", new Guid("d0000000-5500-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Depreciación bienes de uso", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5600-0100-0000-000000000001"), "5.6.1", new Guid("d0000000-5600-0000-0000-000000000001"), true, false, "Activa", "Deudora", "IRAE", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-5600-0200-0000-000000000001"), "5.6.2", new Guid("d0000000-5600-0000-0000-000000000001"), true, false, "Activa", "Deudora", "Otros impuestos", new Guid("d0000000-0001-0001-0001-000000000001"), "Egreso" },
                    { new Guid("d0000000-2100-0400-0100-000000000001"), "2.1.4.1", new Guid("d0000000-2100-0400-0000-000000000001"), true, false, "Activa", "Acreedora", "IRPF a pagar", new Guid("d0000000-0001-0001-0001-000000000001"), "Pasivo" },
                    { new Guid("d0000000-2100-0400-0200-000000000001"), "2.1.4.2", new Guid("d0000000-2100-0400-0000-000000000001"), true, false, "Activa", "Acreedora", "BPS a pagar", new Guid("d0000000-0001-0001-0001-000000000001"), "Pasivo" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1100-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1100-0200-0100-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1100-0200-0200-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1100-0300-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1100-0400-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1100-0500-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1100-0600-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1200-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1200-0200-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1200-0300-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1200-0400-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1200-0500-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2100-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2100-0200-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2100-0300-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2100-0400-0100-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2100-0400-0200-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2100-0500-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2100-0600-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2200-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2200-0200-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-3100-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-3100-0200-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-3200-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-3200-0200-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-4100-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-4100-0200-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-4100-0300-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-4200-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-4200-0200-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-4200-0300-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5100-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5200-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5200-0200-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5200-0300-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5300-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5300-0200-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5300-0300-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5300-0400-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5300-0500-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5400-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5400-0200-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5400-0300-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5500-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5600-0100-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5600-0200-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1100-0200-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2100-0400-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2200-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-3100-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-3200-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-4100-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-4200-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5100-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5200-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5300-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5400-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5500-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-5600-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-2100-0000-0000-000000000001"));

            migrationBuilder.UpdateData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1100-0000-0000-000000000001"),
                columns: new[] { "EsImputable", "Nombre" },
                values: new object[] { true, "Caja" });

            migrationBuilder.UpdateData(
                table: "CuentasContables",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-1200-0000-0000-000000000001"),
                columns: new[] { "EsImputable", "Nombre" },
                values: new object[] { true, "Bancos" });
        }
    }
}
