using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoIntegrador.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtenderSaldoCuentaMultiMoneda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DebeAcumuladoBase",
                table: "SaldosCuenta",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HaberAcumuladoBase",
                table: "SaldosCuenta",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Moneda",
                table: "SaldosCuenta",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoBase",
                table: "SaldosCuenta",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DebeAcumuladoBase",
                table: "SaldosCuenta");

            migrationBuilder.DropColumn(
                name: "HaberAcumuladoBase",
                table: "SaldosCuenta");

            migrationBuilder.DropColumn(
                name: "Moneda",
                table: "SaldosCuenta");

            migrationBuilder.DropColumn(
                name: "SaldoBase",
                table: "SaldosCuenta");
        }
    }
}
