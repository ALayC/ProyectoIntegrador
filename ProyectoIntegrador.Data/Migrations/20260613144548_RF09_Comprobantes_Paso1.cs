using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoIntegrador.Data.Migrations
{
    /// <inheritdoc />
    public partial class RF09_Comprobantes_Paso1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comprobantes_Importaciones_ImportacionId",
                table: "Comprobantes");

            migrationBuilder.DropIndex(
                name: "IX_Comprobantes_ClienteId",
                table: "Comprobantes");

            migrationBuilder.DropIndex(
                name: "IX_Comprobantes_ImportacionId",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "FechaContable",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "FechaEmision",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "ImportacionId",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "Moneda",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "RutContraparte",
                table: "Comprobantes");

            migrationBuilder.RenameColumn(
                name: "TasaIva",
                table: "Comprobantes",
                newName: "TasaIVA");

            migrationBuilder.RenameColumn(
                name: "ImporteIva",
                table: "Comprobantes",
                newName: "ImporteIVA");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Comprobantes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                table: "Comprobantes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Comprobantes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Comprobantes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Comprobantes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "Fecha",
                table: "Comprobantes",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<decimal>(
                name: "ImporteTotal",
                table: "Comprobantes",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RUT",
                table: "Comprobantes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Comprobantes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_ClienteId_Fecha",
                table: "Comprobantes",
                columns: new[] { "ClienteId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_ClienteId_Numero_RUT_Fecha",
                table: "Comprobantes",
                columns: new[] { "ClienteId", "Numero", "RUT", "Fecha" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comprobantes_ClienteId_Fecha",
                table: "Comprobantes");

            migrationBuilder.DropIndex(
                name: "IX_Comprobantes_ClienteId_Numero_RUT_Fecha",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "Fecha",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "ImporteTotal",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "RUT",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Comprobantes");

            migrationBuilder.RenameColumn(
                name: "TasaIVA",
                table: "Comprobantes",
                newName: "TasaIva");

            migrationBuilder.RenameColumn(
                name: "ImporteIVA",
                table: "Comprobantes",
                newName: "ImporteIva");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Comprobantes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Numero",
                table: "Comprobantes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaContable",
                table: "Comprobantes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEmision",
                table: "Comprobantes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ImportacionId",
                table: "Comprobantes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Moneda",
                table: "Comprobantes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RutContraparte",
                table: "Comprobantes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_ClienteId",
                table: "Comprobantes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_ImportacionId",
                table: "Comprobantes",
                column: "ImportacionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comprobantes_Importaciones_ImportacionId",
                table: "Comprobantes",
                column: "ImportacionId",
                principalTable: "Importaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
