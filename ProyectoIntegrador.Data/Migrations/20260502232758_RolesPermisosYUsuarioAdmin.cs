using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProyectoIntegrador.Data.Migrations
{
    /// <inheritdoc />
    public partial class RolesPermisosYUsuarioAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0002-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0002-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0002-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0002-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0003-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0003-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0003-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0003-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0004-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0004-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0004-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0005-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0005-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0005-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0006-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0006-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0007-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0007-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0008-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0008-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0008-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0008-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0009-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0009-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0009-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.DeleteData(
                table: "RolPermisos",
                keyColumns: new[] { "PermisoId", "RolId" },
                keyValues: new object[] { new Guid("b0000000-0009-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "ContadorId", "CreatedAt", "Email", "Estado", "NombreCompleto", "PasswordHash", "ProveedorAuth", "RolId" },
                values: new object[] { new Guid("c0000000-0001-0001-0001-000000000001"), null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@sistema.com", "Activo", "Administrador del Sistema", "$2a$12$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi", "Local", new Guid("a1b2c3d4-0001-0001-0001-000000000001") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0001-0001-0001-000000000001"));

            migrationBuilder.InsertData(
                table: "RolPermisos",
                columns: new[] { "PermisoId", "RolId" },
                values: new object[,]
                {
                    { new Guid("b0000000-0002-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0002-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0002-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0002-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0003-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0003-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0003-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0003-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0004-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0004-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0004-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0005-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0005-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0005-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0006-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0006-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0007-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0007-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0008-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0008-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0008-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0008-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0009-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0009-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0009-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0009-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") }
                });
        }
    }
}
