using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoIntegrador.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailSecurityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmado",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaExpiracionTokenConfirmacion",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaExpiracionTokenRestablecimiento",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenConfirmacionEmail",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenRestablecimiento",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0001-0001-0001-000000000001"),
                columns: new[] { "EmailConfirmado", "FechaExpiracionTokenConfirmacion", "FechaExpiracionTokenRestablecimiento", "TokenConfirmacionEmail", "TokenRestablecimiento" },
                values: new object[] { false, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FechaExpiracionTokenConfirmacion",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FechaExpiracionTokenRestablecimiento",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenConfirmacionEmail",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenRestablecimiento",
                table: "Usuarios");
        }
    }
}
