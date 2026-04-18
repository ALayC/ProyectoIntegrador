using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProyectoIntegrador.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsPredefinido = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposDeCambio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    FuenteOrigen = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposDeCambio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolPermisos",
                columns: table => new
                {
                    RolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermisoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolPermisos", x => new { x.RolId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_RolPermisos_Permisos_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolPermisos_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProveedorAuth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContadorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Usuarios_Usuarios_ContadorId",
                        column: x => x.ContadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Entidad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatosAnteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatosNuevos = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Auditorias_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContadorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rut = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RazonSocial = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreFantasia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoContribuyente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonedaBase = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_Usuarios_ContadorId",
                        column: x => x.ContadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TokensRevocados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiraEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokensRevocados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokensRevocados_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CentrosDeCosto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CentrosDeCosto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CentrosDeCosto_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EjerciciosContables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EjerciciosContables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EjerciciosContables_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Importaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreArchivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaImportacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistrosImportados = table.Column<int>(type: "int", nullable: false),
                    RegistrosRechazados = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Importaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Importaciones_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Importaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlanesDeCuentas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesDeCuentas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanesDeCuentas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AsientosContables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EjercicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AsientoOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Glosa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsientosContables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsientosContables_AsientosContables_AsientoOrigenId",
                        column: x => x.AsientoOrigenId,
                        principalTable: "AsientosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsientosContables_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsientosContables_EjerciciosContables_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "EjerciciosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsientosContables_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuentasContables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanCuentasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaPadreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Naturaleza = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EsImputable = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasContables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasContables_CuentasContables_CuentaPadreId",
                        column: x => x.CuentaPadreId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasContables_PlanesDeCuentas_PlanCuentasId",
                        column: x => x.PlanCuentasId,
                        principalTable: "PlanesDeCuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comprobantes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportacionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AsientoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaContable = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RutContraparte = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImporteNeto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TasaIva = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ImporteIva = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comprobantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comprobantes_AsientosContables_AsientoId",
                        column: x => x.AsientoId,
                        principalTable: "AsientosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comprobantes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comprobantes_Importaciones_ImportacionId",
                        column: x => x.ImportacionId,
                        principalTable: "Importaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LineasAsiento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AsientoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CentroCostoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Debe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Haber = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    ImporteMonedaBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineasAsiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineasAsiento_AsientosContables_AsientoId",
                        column: x => x.AsientoId,
                        principalTable: "AsientosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LineasAsiento_CentrosDeCosto_CentroCostoId",
                        column: x => x.CentroCostoId,
                        principalTable: "CentrosDeCosto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LineasAsiento_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SaldosCuenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuentaContableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EjercicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Periodo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DebeAcumulado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HaberAcumulado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Saldo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaldosCuenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaldosCuenta_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaldosCuenta_CuentasContables_CuentaContableId",
                        column: x => x.CuentaContableId,
                        principalTable: "CuentasContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaldosCuenta_EjerciciosContables_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "EjerciciosContables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Permisos",
                columns: new[] { "Id", "Accion", "Modulo", "Nombre" },
                values: new object[,]
                {
                    { new Guid("b0000000-0001-0001-0001-000000000001"), "Crear", "Usuarios", "Crear Usuarios" },
                    { new Guid("b0000000-0001-0001-0001-000000000002"), "Consultar", "Usuarios", "Consultar Usuarios" },
                    { new Guid("b0000000-0001-0001-0001-000000000003"), "Editar", "Usuarios", "Editar Usuarios" },
                    { new Guid("b0000000-0001-0001-0001-000000000004"), "Desactivar", "Usuarios", "Desactivar Usuarios" },
                    { new Guid("b0000000-0002-0001-0001-000000000001"), "Crear", "Clientes", "Crear Clientes" },
                    { new Guid("b0000000-0002-0001-0001-000000000002"), "Consultar", "Clientes", "Consultar Clientes" },
                    { new Guid("b0000000-0002-0001-0001-000000000003"), "Editar", "Clientes", "Editar Clientes" },
                    { new Guid("b0000000-0002-0001-0001-000000000004"), "Desactivar", "Clientes", "Desactivar Clientes" },
                    { new Guid("b0000000-0003-0001-0001-000000000001"), "Crear", "Cuentas", "Crear Cuentas" },
                    { new Guid("b0000000-0003-0001-0001-000000000002"), "Consultar", "Cuentas", "Consultar Cuentas" },
                    { new Guid("b0000000-0003-0001-0001-000000000003"), "Editar", "Cuentas", "Editar Cuentas" },
                    { new Guid("b0000000-0003-0001-0001-000000000004"), "Desactivar", "Cuentas", "Desactivar Cuentas" },
                    { new Guid("b0000000-0004-0001-0001-000000000001"), "Crear", "Asientos", "Crear Asientos" },
                    { new Guid("b0000000-0004-0001-0001-000000000002"), "Consultar", "Asientos", "Consultar Asientos" },
                    { new Guid("b0000000-0004-0001-0001-000000000003"), "Revertir", "Asientos", "Revertir Asientos" },
                    { new Guid("b0000000-0005-0001-0001-000000000001"), "Crear", "Comprobantes", "Crear Comprobantes" },
                    { new Guid("b0000000-0005-0001-0001-000000000002"), "Consultar", "Comprobantes", "Consultar Comprobantes" },
                    { new Guid("b0000000-0005-0001-0001-000000000003"), "Editar", "Comprobantes", "Editar Comprobantes" },
                    { new Guid("b0000000-0006-0001-0001-000000000001"), "Crear", "Importaciones", "Crear Importaciones" },
                    { new Guid("b0000000-0006-0001-0001-000000000002"), "Consultar", "Importaciones", "Consultar Importaciones" },
                    { new Guid("b0000000-0007-0001-0001-000000000001"), "Consultar", "Reportes", "Consultar Reportes" },
                    { new Guid("b0000000-0007-0001-0001-000000000002"), "Exportar", "Reportes", "Exportar Reportes" },
                    { new Guid("b0000000-0008-0001-0001-000000000001"), "Crear", "Ejercicios", "Crear Ejercicios" },
                    { new Guid("b0000000-0008-0001-0001-000000000002"), "Consultar", "Ejercicios", "Consultar Ejercicios" },
                    { new Guid("b0000000-0008-0001-0001-000000000003"), "Editar", "Ejercicios", "Editar Ejercicios" },
                    { new Guid("b0000000-0008-0001-0001-000000000004"), "Desactivar", "Ejercicios", "Desactivar Ejercicios" },
                    { new Guid("b0000000-0009-0001-0001-000000000001"), "Crear", "CentrosCosto", "Crear Centros de Costo" },
                    { new Guid("b0000000-0009-0001-0001-000000000002"), "Consultar", "CentrosCosto", "Consultar Centros de Costo" },
                    { new Guid("b0000000-0009-0001-0001-000000000003"), "Editar", "CentrosCosto", "Editar Centros de Costo" },
                    { new Guid("b0000000-0009-0001-0001-000000000004"), "Desactivar", "CentrosCosto", "Desactivar Centros de Costo" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "EsPredefinido", "Nombre" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-0001-0001-0001-000000000001"), true, "Administrador" },
                    { new Guid("a1b2c3d4-0001-0001-0001-000000000002"), true, "Contador" },
                    { new Guid("a1b2c3d4-0001-0001-0001-000000000003"), true, "Auxiliar Contable" }
                });

            migrationBuilder.InsertData(
                table: "RolPermisos",
                columns: new[] { "PermisoId", "RolId" },
                values: new object[,]
                {
                    { new Guid("b0000000-0001-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0001-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0001-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0001-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
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
                    { new Guid("b0000000-0009-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000001") },
                    { new Guid("b0000000-0002-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0002-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0002-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0002-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0003-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0003-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0003-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0003-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0004-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0004-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0004-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0005-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0005-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0005-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0006-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0006-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0007-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0007-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0008-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0008-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0008-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0008-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0009-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0009-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0009-0001-0001-000000000003"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0009-0001-0001-000000000004"), new Guid("a1b2c3d4-0001-0001-0001-000000000002") },
                    { new Guid("b0000000-0001-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000003") },
                    { new Guid("b0000000-0002-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000003") },
                    { new Guid("b0000000-0003-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000003") },
                    { new Guid("b0000000-0004-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000003") },
                    { new Guid("b0000000-0004-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000003") },
                    { new Guid("b0000000-0005-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000003") },
                    { new Guid("b0000000-0005-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000003") },
                    { new Guid("b0000000-0006-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000003") },
                    { new Guid("b0000000-0007-0001-0001-000000000001"), new Guid("a1b2c3d4-0001-0001-0001-000000000003") },
                    { new Guid("b0000000-0008-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000003") },
                    { new Guid("b0000000-0009-0001-0001-000000000002"), new Guid("a1b2c3d4-0001-0001-0001-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_AsientoOrigenId",
                table: "AsientosContables",
                column: "AsientoOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_ClienteId_EjercicioId_Fecha",
                table: "AsientosContables",
                columns: new[] { "ClienteId", "EjercicioId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_EjercicioId",
                table: "AsientosContables",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_AsientosContables_UsuarioId",
                table: "AsientosContables",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_UsuarioId",
                table: "Auditorias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CentrosDeCosto_ClienteId_Codigo",
                table: "CentrosDeCosto",
                columns: new[] { "ClienteId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_ContadorId",
                table: "Clientes",
                column: "ContadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Rut",
                table: "Clientes",
                column: "Rut",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_AsientoId",
                table: "Comprobantes",
                column: "AsientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_ClienteId",
                table: "Comprobantes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_ImportacionId",
                table: "Comprobantes",
                column: "ImportacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_CuentaPadreId",
                table: "CuentasContables",
                column: "CuentaPadreId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasContables_PlanCuentasId_Codigo",
                table: "CuentasContables",
                columns: new[] { "PlanCuentasId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EjerciciosContables_ClienteId",
                table: "EjerciciosContables",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Importaciones_ClienteId",
                table: "Importaciones",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Importaciones_UsuarioId",
                table: "Importaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_LineasAsiento_AsientoId_CuentaContableId",
                table: "LineasAsiento",
                columns: new[] { "AsientoId", "CuentaContableId" });

            migrationBuilder.CreateIndex(
                name: "IX_LineasAsiento_CentroCostoId",
                table: "LineasAsiento",
                column: "CentroCostoId");

            migrationBuilder.CreateIndex(
                name: "IX_LineasAsiento_CuentaContableId",
                table: "LineasAsiento",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanesDeCuentas_ClienteId",
                table: "PlanesDeCuentas",
                column: "ClienteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_PermisoId",
                table: "RolPermisos",
                column: "PermisoId");

            migrationBuilder.CreateIndex(
                name: "IX_SaldosCuenta_ClienteId_CuentaContableId_Periodo",
                table: "SaldosCuenta",
                columns: new[] { "ClienteId", "CuentaContableId", "Periodo" });

            migrationBuilder.CreateIndex(
                name: "IX_SaldosCuenta_CuentaContableId",
                table: "SaldosCuenta",
                column: "CuentaContableId");

            migrationBuilder.CreateIndex(
                name: "IX_SaldosCuenta_EjercicioId",
                table: "SaldosCuenta",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposDeCambio_Moneda_Fecha",
                table: "TiposDeCambio",
                columns: new[] { "Moneda", "Fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TokensRevocados_UsuarioId",
                table: "TokensRevocados",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_ContadorId",
                table: "Usuarios",
                column: "ContadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditorias");

            migrationBuilder.DropTable(
                name: "Comprobantes");

            migrationBuilder.DropTable(
                name: "LineasAsiento");

            migrationBuilder.DropTable(
                name: "RolPermisos");

            migrationBuilder.DropTable(
                name: "SaldosCuenta");

            migrationBuilder.DropTable(
                name: "TiposDeCambio");

            migrationBuilder.DropTable(
                name: "TokensRevocados");

            migrationBuilder.DropTable(
                name: "Importaciones");

            migrationBuilder.DropTable(
                name: "AsientosContables");

            migrationBuilder.DropTable(
                name: "CentrosDeCosto");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "CuentasContables");

            migrationBuilder.DropTable(
                name: "EjerciciosContables");

            migrationBuilder.DropTable(
                name: "PlanesDeCuentas");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
