using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Context;

public static class SeedData
{
    // ──────────────────────────────────────────────
    // IDs fijos para el template de PlanDeCuentas
    // ──────────────────────────────────────────────
    public static readonly Guid PlanTemplateId = new("d0000000-0001-0001-0001-000000000001");

    // Nivel 1
    public static readonly Guid ActivoId = new("d0000000-1000-0000-0000-000000000001");
    public static readonly Guid PasivoId = new("d0000000-2000-0000-0000-000000000001");
    public static readonly Guid PatrimonioId = new("d0000000-3000-0000-0000-000000000001");
    public static readonly Guid IngresosId = new("d0000000-4000-0000-0000-000000000001");
    public static readonly Guid EgresosId = new("d0000000-5000-0000-0000-000000000001");

    // Nivel 2 – Activo
    public static readonly Guid ActivoCorrienteId   = new("d0000000-1100-0000-0000-000000000001");
    public static readonly Guid ActivoNoCorrienteId = new("d0000000-1200-0000-0000-000000000001");

    // Nivel 2 – Pasivo
    public static readonly Guid PasivoCorrienteId   = new("d0000000-2100-0000-0000-000000000001");
    public static readonly Guid PasivoNoCorrienteId = new("d0000000-2200-0000-0000-000000000001");

    // Nivel 2 – Patrimonio
    public static readonly Guid CapitalId    = new("d0000000-3100-0000-0000-000000000001");
    public static readonly Guid ResultadosId = new("d0000000-3200-0000-0000-000000000001");

    // Nivel 2 – Ingresos
    public static readonly Guid IngresosOperativosId   = new("d0000000-4100-0000-0000-000000000001");
    public static readonly Guid IngresosNoOperativosId = new("d0000000-4200-0000-0000-000000000001");

    // Nivel 2 – Egresos
    public static readonly Guid CostosId             = new("d0000000-5100-0000-0000-000000000001");
    public static readonly Guid GastosPersonalId     = new("d0000000-5200-0000-0000-000000000001");
    public static readonly Guid GastosGeneralesId    = new("d0000000-5300-0000-0000-000000000001");
    public static readonly Guid GastosFinancierosId  = new("d0000000-5400-0000-0000-000000000001");
    public static readonly Guid DepreciacionesId     = new("d0000000-5500-0000-0000-000000000001");
    public static readonly Guid ImpuestosId          = new("d0000000-5600-0000-0000-000000000001");

    // Nivel 3 – Activo Corriente (1.1.X)
    public static readonly Guid CajaId                 = new("d0000000-1100-0100-0000-000000000001");
    public static readonly Guid BancosId               = new("d0000000-1100-0200-0000-000000000001");
    public static readonly Guid ClientesACobrarId      = new("d0000000-1100-0300-0000-000000000001");
    public static readonly Guid DeudoresVariosId       = new("d0000000-1100-0400-0000-000000000001");
    public static readonly Guid IvaCreditoFiscalId     = new("d0000000-1100-0500-0000-000000000001");
    public static readonly Guid AnticiposProveedoresId = new("d0000000-1100-0600-0000-000000000001");

    // Nivel 3 – Activo No Corriente (1.2.X)
    public static readonly Guid InmueblesId             = new("d0000000-1200-0100-0000-000000000001");
    public static readonly Guid MueblesUtilesId         = new("d0000000-1200-0200-0000-000000000001");
    public static readonly Guid EquiposComputacionId    = new("d0000000-1200-0300-0000-000000000001");
    public static readonly Guid RodadosId               = new("d0000000-1200-0400-0000-000000000001");
    public static readonly Guid DepreciacionAcumuladaId = new("d0000000-1200-0500-0000-000000000001");

    // Nivel 3 – Pasivo Corriente (2.1.X)
    public static readonly Guid ProveedoresAPagarId = new("d0000000-2100-0100-0000-000000000001");
    public static readonly Guid AcreedoresVariosId  = new("d0000000-2100-0200-0000-000000000001");
    public static readonly Guid IvaDebitoFiscalId   = new("d0000000-2100-0300-0000-000000000001");
    public static readonly Guid RetencionesPagarId  = new("d0000000-2100-0400-0000-000000000001");
    public static readonly Guid SueldosPagarId      = new("d0000000-2100-0500-0000-000000000001");
    public static readonly Guid AnticiposClientesId = new("d0000000-2100-0600-0000-000000000001");

    // Nivel 3 – Pasivo No Corriente (2.2.X)
    public static readonly Guid PrestamosBancariosLPId = new("d0000000-2200-0100-0000-000000000001");
    public static readonly Guid OtrasDeudasLPId        = new("d0000000-2200-0200-0000-000000000001");

    // Nivel 3 – Capital (3.1.X)
    public static readonly Guid CapitalSocialId       = new("d0000000-3100-0100-0000-000000000001");
    public static readonly Guid AportesIrrevocablesId = new("d0000000-3100-0200-0000-000000000001");

    // Nivel 3 – Resultados (3.2.X)
    public static readonly Guid ResultadosAcumuladosId = new("d0000000-3200-0100-0000-000000000001");
    public static readonly Guid ResultadoEjercicioId   = new("d0000000-3200-0200-0000-000000000001");

    // Nivel 3 – Ingresos Operativos (4.1.X)
    public static readonly Guid VentasMercaderiaId    = new("d0000000-4100-0100-0000-000000000001");
    public static readonly Guid VentasServiciosId     = new("d0000000-4100-0200-0000-000000000001");
    public static readonly Guid DescuentosObtenidosId = new("d0000000-4100-0300-0000-000000000001");

    // Nivel 3 – Ingresos No Operativos (4.2.X)
    public static readonly Guid InteresesGanadosId        = new("d0000000-4200-0100-0000-000000000001");
    public static readonly Guid DiferenciaCambioGanadaId  = new("d0000000-4200-0200-0000-000000000001");
    public static readonly Guid OtrosIngresosId           = new("d0000000-4200-0300-0000-000000000001");

    // Nivel 3 – Costos (5.1.X)
    public static readonly Guid CostoMercaderiaVendidaId = new("d0000000-5100-0100-0000-000000000001");

    // Nivel 3 – Gastos de Personal (5.2.X)
    public static readonly Guid SueldosJornalesId     = new("d0000000-5200-0100-0000-000000000001");
    public static readonly Guid AportesPatronalesId   = new("d0000000-5200-0200-0000-000000000001");
    public static readonly Guid OtrosGastosPersonalId = new("d0000000-5200-0300-0000-000000000001");

    // Nivel 3 – Gastos Generales (5.3.X)
    public static readonly Guid AlquilerId                  = new("d0000000-5300-0100-0000-000000000001");
    public static readonly Guid ServiciosId                 = new("d0000000-5300-0200-0000-000000000001");
    public static readonly Guid PapeleriaUtilesId           = new("d0000000-5300-0300-0000-000000000001");
    public static readonly Guid MantenimientoReparacionesId = new("d0000000-5300-0400-0000-000000000001");
    public static readonly Guid SeguroId                    = new("d0000000-5300-0500-0000-000000000001");

    // Nivel 3 – Gastos Financieros (5.4.X)
    public static readonly Guid InteresesPagadosId        = new("d0000000-5400-0100-0000-000000000001");
    public static readonly Guid ComisionesBancariasId     = new("d0000000-5400-0200-0000-000000000001");
    public static readonly Guid DiferenciaCambioPerdidaId = new("d0000000-5400-0300-0000-000000000001");

    // Nivel 3 – Depreciaciones (5.5.X)
    public static readonly Guid DepreciacionBienesUsoId = new("d0000000-5500-0100-0000-000000000001");

    // Nivel 3 – Impuestos (5.6.X)
    public static readonly Guid IraeId           = new("d0000000-5600-0100-0000-000000000001");
    public static readonly Guid OtrosImpuestosId = new("d0000000-5600-0200-0000-000000000001");

    // Nivel 4 – Bancos (1.1.2.X)
    public static readonly Guid BancoBROUId = new("d0000000-1100-0200-0100-000000000001");
    public static readonly Guid BancoITAUId = new("d0000000-1100-0200-0200-000000000001");

    // Nivel 4 – Retenciones a pagar (2.1.4.X)
    public static readonly Guid IrpfPagarId = new("d0000000-2100-0400-0100-000000000001");
    public static readonly Guid BpsPagarId  = new("d0000000-2100-0400-0200-000000000001");

    // ──────────────────────────────────────────────
    // IDs fijos de Roles
    // ──────────────────────────────────────────────
    public static readonly Guid RolAdministradorId = new("a1b2c3d4-0001-0001-0001-000000000001");
    public static readonly Guid RolContadorId = new("a1b2c3d4-0001-0001-0001-000000000002");
    public static readonly Guid RolAuxiliarId = new("a1b2c3d4-0001-0001-0001-000000000003");

    // ──────────────────────────────────────────────
    // IDs fijos de Permisos
    // Convención: módulo-acción en el Guid seed
    // ──────────────────────────────────────────────

    // Usuarios
    private static readonly Guid PermUsuariosCrear = new("b0000000-0001-0001-0001-000000000001");
    private static readonly Guid PermUsuariosConsultar = new("b0000000-0001-0001-0001-000000000002");
    private static readonly Guid PermUsuariosEditar = new("b0000000-0001-0001-0001-000000000003");
    private static readonly Guid PermUsuariosDesactivar = new("b0000000-0001-0001-0001-000000000004");

    // Clientes
    private static readonly Guid PermClientesCrear = new("b0000000-0002-0001-0001-000000000001");
    private static readonly Guid PermClientesConsultar = new("b0000000-0002-0001-0001-000000000002");
    private static readonly Guid PermClientesEditar = new("b0000000-0002-0001-0001-000000000003");
    private static readonly Guid PermClientesDesactivar = new("b0000000-0002-0001-0001-000000000004");

    // Cuentas
    private static readonly Guid PermCuentasCrear = new("b0000000-0003-0001-0001-000000000001");
    private static readonly Guid PermCuentasConsultar = new("b0000000-0003-0001-0001-000000000002");
    private static readonly Guid PermCuentasEditar = new("b0000000-0003-0001-0001-000000000003");
    private static readonly Guid PermCuentasDesactivar = new("b0000000-0003-0001-0001-000000000004");

    // Asientos
    private static readonly Guid PermAsientosCrear = new("b0000000-0004-0001-0001-000000000001");
    private static readonly Guid PermAsientosConsultar = new("b0000000-0004-0001-0001-000000000002");
    private static readonly Guid PermAsientosRevertir = new("b0000000-0004-0001-0001-000000000003");

    // Comprobantes
    private static readonly Guid PermComprobantesCrear = new("b0000000-0005-0001-0001-000000000001");
    private static readonly Guid PermComprobantesConsultar = new("b0000000-0005-0001-0001-000000000002");
    private static readonly Guid PermComprobantesEditar = new("b0000000-0005-0001-0001-000000000003");
    private static readonly Guid PermComprobantesAnular = new("b0000000-0005-0001-0001-000000000004");

    // Importaciones
    private static readonly Guid PermImportacionesCrear = new("b0000000-0006-0001-0001-000000000001");
    private static readonly Guid PermImportacionesConsultar = new("b0000000-0006-0001-0001-000000000002");

    // Reportes
    private static readonly Guid PermReportesConsultar = new("b0000000-0007-0001-0001-000000000001");
    private static readonly Guid PermReportesExportar = new("b0000000-0007-0001-0001-000000000002");

    // Ejercicios
    private static readonly Guid PermEjerciciosCrear = new("b0000000-0008-0001-0001-000000000001");
    private static readonly Guid PermEjerciciosConsultar = new("b0000000-0008-0001-0001-000000000002");
    private static readonly Guid PermEjerciciosEditar = new("b0000000-0008-0001-0001-000000000003");
    private static readonly Guid PermEjerciciosDesactivar = new("b0000000-0008-0001-0001-000000000004");

    // CentrosCosto
    private static readonly Guid PermCentrosCostoCrear = new("b0000000-0009-0001-0001-000000000001");
    private static readonly Guid PermCentrosCostoConsultar = new("b0000000-0009-0001-0001-000000000002");
    private static readonly Guid PermCentrosCostoEditar = new("b0000000-0009-0001-0001-000000000003");
    private static readonly Guid PermCentrosCostoDesactivar = new("b0000000-0009-0001-0001-000000000004");

    // ID fijo del usuario Administrador
    public static readonly Guid UsuarioAdminId = new("c0000000-0001-0001-0001-000000000001");

    // ──────────────────────────────────────────────
    // Método principal de seed
    // ──────────────────────────────────────────────
    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedRoles(modelBuilder);
        SeedPermisos(modelBuilder);
        SeedRolPermisos(modelBuilder);
        SeedUsuarioAdmin(modelBuilder);
        SeedPlanDeCuentasTemplate(modelBuilder);
        SeedCuentasContablesTemplate(modelBuilder);
    }

    // ──────────────────────────────────────────────
    // Roles predefinidos
    // ──────────────────────────────────────────────
    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>().HasData(
      new Rol { Id = RolAdministradorId, Nombre = "Administrador", EsPredefinido = true },
   new Rol { Id = RolContadorId, Nombre = "Contador", EsPredefinido = true },
            new Rol { Id = RolAuxiliarId, Nombre = "Auxiliar Contable", EsPredefinido = true }
        );
    }

    // ──────────────────────────────────────────────
    // Permisos por módulo y acción
    // ──────────────────────────────────────────────
    private static void SeedPermisos(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permiso>().HasData(
         // ── Usuarios ──
         new Permiso { Id = PermUsuariosCrear, Nombre = "Crear Usuarios", Modulo = "Usuarios", Accion = "Crear" },
             new Permiso { Id = PermUsuariosConsultar, Nombre = "Consultar Usuarios", Modulo = "Usuarios", Accion = "Consultar" },
               new Permiso { Id = PermUsuariosEditar, Nombre = "Editar Usuarios", Modulo = "Usuarios", Accion = "Editar" },
               new Permiso { Id = PermUsuariosDesactivar, Nombre = "Desactivar Usuarios", Modulo = "Usuarios", Accion = "Desactivar" },

          // ── Clientes ──
          new Permiso { Id = PermClientesCrear, Nombre = "Crear Clientes", Modulo = "Clientes", Accion = "Crear" },
               new Permiso { Id = PermClientesConsultar, Nombre = "Consultar Clientes", Modulo = "Clientes", Accion = "Consultar" },
          new Permiso { Id = PermClientesEditar, Nombre = "Editar Clientes", Modulo = "Clientes", Accion = "Editar" },
               new Permiso { Id = PermClientesDesactivar, Nombre = "Desactivar Clientes", Modulo = "Clientes", Accion = "Desactivar" },

          // ── Cuentas ──
          new Permiso { Id = PermCuentasCrear, Nombre = "Crear Cuentas", Modulo = "Cuentas", Accion = "Crear" },
       new Permiso { Id = PermCuentasConsultar, Nombre = "Consultar Cuentas", Modulo = "Cuentas", Accion = "Consultar" },
               new Permiso { Id = PermCuentasEditar, Nombre = "Editar Cuentas", Modulo = "Cuentas", Accion = "Editar" },
               new Permiso { Id = PermCuentasDesactivar, Nombre = "Desactivar Cuentas", Modulo = "Cuentas", Accion = "Desactivar" },

      // ── Asientos ──
      new Permiso { Id = PermAsientosCrear, Nombre = "Crear Asientos", Modulo = "Asientos", Accion = "Crear" },
               new Permiso { Id = PermAsientosConsultar, Nombre = "Consultar Asientos", Modulo = "Asientos", Accion = "Consultar" },
               new Permiso { Id = PermAsientosRevertir, Nombre = "Revertir Asientos", Modulo = "Asientos", Accion = "Revertir" },

     // ── Comprobantes ──
     new Permiso { Id = PermComprobantesCrear, Nombre = "Crear Comprobantes", Modulo = "Comprobantes", Accion = "Crear" },
               new Permiso { Id = PermComprobantesConsultar, Nombre = "Consultar Comprobantes", Modulo = "Comprobantes", Accion = "Consultar" },
               new Permiso { Id = PermComprobantesEditar, Nombre = "Editar Comprobantes", Modulo = "Comprobantes", Accion = "Editar" },
               new Permiso { Id = PermComprobantesAnular, Nombre = "Anular Comprobantes", Modulo = "Comprobantes", Accion = "Anular" },

          // ── Importaciones ──
          new Permiso { Id = PermImportacionesCrear, Nombre = "Crear Importaciones", Modulo = "Importaciones", Accion = "Crear" },
               new Permiso { Id = PermImportacionesConsultar, Nombre = "Consultar Importaciones", Modulo = "Importaciones", Accion = "Consultar" },

               // ── Reportes ──
               new Permiso { Id = PermReportesConsultar, Nombre = "Consultar Reportes", Modulo = "Reportes", Accion = "Consultar" },
               new Permiso { Id = PermReportesExportar, Nombre = "Exportar Reportes", Modulo = "Reportes", Accion = "Exportar" },

    // ── Ejercicios ──
    new Permiso { Id = PermEjerciciosCrear, Nombre = "Crear Ejercicios", Modulo = "Ejercicios", Accion = "Crear" },
            new Permiso { Id = PermEjerciciosConsultar, Nombre = "Consultar Ejercicios", Modulo = "Ejercicios", Accion = "Consultar" },
             new Permiso { Id = PermEjerciciosEditar, Nombre = "Editar Ejercicios", Modulo = "Ejercicios", Accion = "Editar" },
               new Permiso { Id = PermEjerciciosDesactivar, Nombre = "Desactivar Ejercicios", Modulo = "Ejercicios", Accion = "Desactivar" },

           // ── CentrosCosto ──
           new Permiso { Id = PermCentrosCostoCrear, Nombre = "Crear Centros de Costo", Modulo = "CentrosCosto", Accion = "Crear" },
               new Permiso { Id = PermCentrosCostoConsultar, Nombre = "Consultar Centros de Costo", Modulo = "CentrosCosto", Accion = "Consultar" },
    new Permiso { Id = PermCentrosCostoEditar, Nombre = "Editar Centros de Costo", Modulo = "CentrosCosto", Accion = "Editar" },
        new Permiso { Id = PermCentrosCostoDesactivar, Nombre = "Desactivar Centros de Costo", Modulo = "CentrosCosto", Accion = "Desactivar" }
           );
    }

    // ──────────────────────────────────────────────
    // Asignación de permisos por rol
    // ──────────────────────────────────────────────
    private static void SeedRolPermisos(ModelBuilder modelBuilder)
    {
        // Permisos del módulo Usuarios
        var permisosUsuarios = new[]
      {
            PermUsuariosCrear, PermUsuariosConsultar, PermUsuariosEditar, PermUsuariosDesactivar
        };

        // Todos los permisos
        var todosLosPermisos = new[]
   {
 PermUsuariosCrear, PermUsuariosConsultar, PermUsuariosEditar, PermUsuariosDesactivar,
            PermClientesCrear, PermClientesConsultar, PermClientesEditar, PermClientesDesactivar,
         PermCuentasCrear, PermCuentasConsultar, PermCuentasEditar, PermCuentasDesactivar,
     PermAsientosCrear, PermAsientosConsultar, PermAsientosRevertir,
            PermComprobantesCrear, PermComprobantesConsultar, PermComprobantesEditar, PermComprobantesAnular,
    PermImportacionesCrear, PermImportacionesConsultar,
          PermReportesConsultar, PermReportesExportar,
            PermEjerciciosCrear, PermEjerciciosConsultar, PermEjerciciosEditar, PermEjerciciosDesactivar,
    PermCentrosCostoCrear, PermCentrosCostoConsultar, PermCentrosCostoEditar, PermCentrosCostoDesactivar
  };

        // Administrador: SOLO módulo Usuarios (Visión 2)
        var rolPermisosAdmin = permisosUsuarios
            .Select(permisoId => new RolPermiso { RolId = RolAdministradorId, PermisoId = permisoId });

        // Contador: todos excepto Usuarios
        var permisosContador = todosLosPermisos.Except(permisosUsuarios).ToArray();
        var rolPermisosContador = permisosContador
              .Select(permisoId => new RolPermiso { RolId = RolContadorId, PermisoId = permisoId });

        // Auxiliar: consultar todos + crear asientos y comprobantes
        var permisosAuxiliar = new[]
           {
          PermUsuariosConsultar,
      PermClientesConsultar,
       PermCuentasConsultar,
   PermAsientosConsultar, PermAsientosCrear,
          PermComprobantesConsultar, PermComprobantesCrear,
  PermImportacionesConsultar,
    PermReportesConsultar,
     PermEjerciciosConsultar,
            PermCentrosCostoConsultar
   };
        var rolPermisosAuxiliar = permisosAuxiliar
  .Select(permisoId => new RolPermiso { RolId = RolAuxiliarId, PermisoId = permisoId });

        modelBuilder.Entity<RolPermiso>().HasData(
            rolPermisosAdmin
  .Concat(rolPermisosContador)
   .Concat(rolPermisosAuxiliar)
                .ToArray()
        );
    }

    // ──────────────────────────────────────────────
    // Usuario Administrador predefinido
    // ──────────────────────────────────────────────
    private static void SeedUsuarioAdmin(ModelBuilder modelBuilder)
    {
        // Hash de "Admin1234!" generado con BCrypt workFactor=12
        // Se puede regenerar con: BCrypt.Net.BCrypt.HashPassword("Admin1234!", workFactor: 12)
        const string adminPasswordHash = "$2a$12$siCyK43j/60igAgW0GwTXOojpsf5pt0X6IIu9I5FfBhE645FlNcLW";

        modelBuilder.Entity<Usuario>().HasData(new Usuario
        {
            Id = UsuarioAdminId,
            Email = "admin@sistema.com",
            PasswordHash = adminPasswordHash,
            NombreCompleto = "Administrador del Sistema",
            ProveedorAuth = "Local",
            Estado = "Activo",
            RolId = RolAdministradorId,
            ContadorId = null,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }

    private static void SeedPlanDeCuentasTemplate(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlanDeCuentas>().HasData(
            new PlanDeCuentas
            {
                Id = PlanTemplateId,
                ClienteId = null,
                EsTemplate = true
            }
        );
    }

    private static void SeedCuentasContablesTemplate(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CuentaContable>().HasData(

            // ─────────────────────────────────────────────────────────────
            // NIVEL 1
            // ─────────────────────────────────────────────────────────────
            new CuentaContable { Id = ActivoId,    PlanCuentasId = PlanTemplateId, CuentaPadreId = null, Codigo = "1", Nombre = "Activo",     Tipo = "Activo",     Naturaleza = "Deudora",   EsImputable = false, EsSistema = true,  Estado = "Activa" },
            new CuentaContable { Id = PasivoId,    PlanCuentasId = PlanTemplateId, CuentaPadreId = null, Codigo = "2", Nombre = "Pasivo",     Tipo = "Pasivo",     Naturaleza = "Acreedora", EsImputable = false, EsSistema = true,  Estado = "Activa" },
            new CuentaContable { Id = PatrimonioId, PlanCuentasId = PlanTemplateId, CuentaPadreId = null, Codigo = "3", Nombre = "Patrimonio", Tipo = "Patrimonio", Naturaleza = "Acreedora", EsImputable = false, EsSistema = true,  Estado = "Activa" },
            new CuentaContable { Id = IngresosId,  PlanCuentasId = PlanTemplateId, CuentaPadreId = null, Codigo = "4", Nombre = "Ingresos",   Tipo = "Ingreso",    Naturaleza = "Acreedora", EsImputable = false, EsSistema = true,  Estado = "Activa" },
            new CuentaContable { Id = EgresosId,   PlanCuentasId = PlanTemplateId, CuentaPadreId = null, Codigo = "5", Nombre = "Egresos",    Tipo = "Egreso",     Naturaleza = "Deudora",   EsImputable = false, EsSistema = true,  Estado = "Activa" },

            // ─────────────────────────────────────────────────────────────
            // NIVEL 2 — Activo
            // ─────────────────────────────────────────────────────────────
            new CuentaContable { Id = ActivoCorrienteId,   PlanCuentasId = PlanTemplateId, CuentaPadreId = ActivoId, Codigo = "1.1", Nombre = "Activo Corriente",    Tipo = "Activo", Naturaleza = "Deudora", EsImputable = false, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = ActivoNoCorrienteId, PlanCuentasId = PlanTemplateId, CuentaPadreId = ActivoId, Codigo = "1.2", Nombre = "Activo No Corriente", Tipo = "Activo", Naturaleza = "Deudora", EsImputable = false, EsSistema = false, Estado = "Activa" },

            // NIVEL 2 — Pasivo
            new CuentaContable { Id = PasivoCorrienteId,   PlanCuentasId = PlanTemplateId, CuentaPadreId = PasivoId, Codigo = "2.1", Nombre = "Pasivo Corriente",    Tipo = "Pasivo", Naturaleza = "Acreedora", EsImputable = false, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = PasivoNoCorrienteId, PlanCuentasId = PlanTemplateId, CuentaPadreId = PasivoId, Codigo = "2.2", Nombre = "Pasivo No Corriente", Tipo = "Pasivo", Naturaleza = "Acreedora", EsImputable = false, EsSistema = false, Estado = "Activa" },

            // NIVEL 2 — Patrimonio
            new CuentaContable { Id = CapitalId,    PlanCuentasId = PlanTemplateId, CuentaPadreId = PatrimonioId, Codigo = "3.1", Nombre = "Capital",     Tipo = "Patrimonio", Naturaleza = "Acreedora", EsImputable = false, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = ResultadosId, PlanCuentasId = PlanTemplateId, CuentaPadreId = PatrimonioId, Codigo = "3.2", Nombre = "Resultados",  Tipo = "Patrimonio", Naturaleza = "Acreedora", EsImputable = false, EsSistema = false, Estado = "Activa" },

            // NIVEL 2 — Ingresos
            new CuentaContable { Id = IngresosOperativosId,   PlanCuentasId = PlanTemplateId, CuentaPadreId = IngresosId, Codigo = "4.1", Nombre = "Ingresos Operativos",    Tipo = "Ingreso", Naturaleza = "Acreedora", EsImputable = false, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = IngresosNoOperativosId, PlanCuentasId = PlanTemplateId, CuentaPadreId = IngresosId, Codigo = "4.2", Nombre = "Ingresos No Operativos", Tipo = "Ingreso", Naturaleza = "Acreedora", EsImputable = false, EsSistema = false, Estado = "Activa" },

            // NIVEL 2 — Egresos
            new CuentaContable { Id = CostosId,            PlanCuentasId = PlanTemplateId, CuentaPadreId = EgresosId, Codigo = "5.1", Nombre = "Costos",                  Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = false, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = GastosPersonalId,    PlanCuentasId = PlanTemplateId, CuentaPadreId = EgresosId, Codigo = "5.2", Nombre = "Gastos de Personal",      Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = false, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = GastosGeneralesId,   PlanCuentasId = PlanTemplateId, CuentaPadreId = EgresosId, Codigo = "5.3", Nombre = "Gastos Generales",        Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = false, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = GastosFinancierosId, PlanCuentasId = PlanTemplateId, CuentaPadreId = EgresosId, Codigo = "5.4", Nombre = "Gastos Financieros",      Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = false, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = DepreciacionesId,    PlanCuentasId = PlanTemplateId, CuentaPadreId = EgresosId, Codigo = "5.5", Nombre = "Depreciaciones",           Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = false, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = ImpuestosId,         PlanCuentasId = PlanTemplateId, CuentaPadreId = EgresosId, Codigo = "5.6", Nombre = "Impuestos y Contribuciones", Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = false, EsSistema = false, Estado = "Activa" },

            // ─────────────────────────────────────────────────────────────
            // NIVEL 3 — Activo Corriente (1.1.X)
            // ─────────────────────────────────────────────────────────────
            new CuentaContable { Id = CajaId,                 PlanCuentasId = PlanTemplateId, CuentaPadreId = ActivoCorrienteId, Codigo = "1.1.1", Nombre = "Caja",                     Tipo = "Activo", Naturaleza = "Deudora", EsImputable = true,  EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = BancosId,               PlanCuentasId = PlanTemplateId, CuentaPadreId = ActivoCorrienteId, Codigo = "1.1.2", Nombre = "Bancos",                   Tipo = "Activo", Naturaleza = "Deudora", EsImputable = false, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = ClientesACobrarId,      PlanCuentasId = PlanTemplateId, CuentaPadreId = ActivoCorrienteId, Codigo = "1.1.3", Nombre = "Clientes a cobrar",        Tipo = "Activo", Naturaleza = "Deudora", EsImputable = true,  EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = DeudoresVariosId,       PlanCuentasId = PlanTemplateId, CuentaPadreId = ActivoCorrienteId, Codigo = "1.1.4", Nombre = "Deudores varios",          Tipo = "Activo", Naturaleza = "Deudora", EsImputable = true,  EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = IvaCreditoFiscalId,     PlanCuentasId = PlanTemplateId, CuentaPadreId = ActivoCorrienteId, Codigo = "1.1.5", Nombre = "IVA Crédito Fiscal",       Tipo = "Activo", Naturaleza = "Deudora", EsImputable = true,  EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = AnticiposProveedoresId, PlanCuentasId = PlanTemplateId, CuentaPadreId = ActivoCorrienteId, Codigo = "1.1.6", Nombre = "Anticipos a proveedores",  Tipo = "Activo", Naturaleza = "Deudora", EsImputable = true,  EsSistema = false, Estado = "Activa" },

            // NIVEL 3 — Activo No Corriente (1.2.X)
            new CuentaContable { Id = InmueblesId,             PlanCuentasId = PlanTemplateId, CuentaPadreId = ActivoNoCorrienteId, Codigo = "1.2.1", Nombre = "Inmuebles",                      Tipo = "Activo", Naturaleza = "Deudora", EsImputable = true,  EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = MueblesUtilesId,         PlanCuentasId = PlanTemplateId, CuentaPadreId = ActivoNoCorrienteId, Codigo = "1.2.2", Nombre = "Muebles y útiles",               Tipo = "Activo", Naturaleza = "Deudora", EsImputable = true,  EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = EquiposComputacionId,    PlanCuentasId = PlanTemplateId, CuentaPadreId = ActivoNoCorrienteId, Codigo = "1.2.3", Nombre = "Equipos de computación",         Tipo = "Activo", Naturaleza = "Deudora", EsImputable = true,  EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = RodadosId,               PlanCuentasId = PlanTemplateId, CuentaPadreId = ActivoNoCorrienteId, Codigo = "1.2.4", Nombre = "Rodados",                        Tipo = "Activo", Naturaleza = "Deudora", EsImputable = true,  EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = DepreciacionAcumuladaId, PlanCuentasId = PlanTemplateId, CuentaPadreId = ActivoNoCorrienteId, Codigo = "1.2.5", Nombre = "Depreciación acumulada (activos)", Tipo = "Activo", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },

            // NIVEL 3 — Pasivo Corriente (2.1.X)
            new CuentaContable { Id = ProveedoresAPagarId, PlanCuentasId = PlanTemplateId, CuentaPadreId = PasivoCorrienteId, Codigo = "2.1.1", Nombre = "Proveedores a pagar",  Tipo = "Pasivo", Naturaleza = "Acreedora", EsImputable = true,  EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = AcreedoresVariosId,  PlanCuentasId = PlanTemplateId, CuentaPadreId = PasivoCorrienteId, Codigo = "2.1.2", Nombre = "Acreedores varios",    Tipo = "Pasivo", Naturaleza = "Acreedora", EsImputable = true,  EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = IvaDebitoFiscalId,   PlanCuentasId = PlanTemplateId, CuentaPadreId = PasivoCorrienteId, Codigo = "2.1.3", Nombre = "IVA Débito Fiscal",    Tipo = "Pasivo", Naturaleza = "Acreedora", EsImputable = true,  EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = RetencionesPagarId,  PlanCuentasId = PlanTemplateId, CuentaPadreId = PasivoCorrienteId, Codigo = "2.1.4", Nombre = "Retenciones a pagar",  Tipo = "Pasivo", Naturaleza = "Acreedora", EsImputable = false, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = SueldosPagarId,      PlanCuentasId = PlanTemplateId, CuentaPadreId = PasivoCorrienteId, Codigo = "2.1.5", Nombre = "Sueldos a pagar",      Tipo = "Pasivo", Naturaleza = "Acreedora", EsImputable = true,  EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = AnticiposClientesId, PlanCuentasId = PlanTemplateId, CuentaPadreId = PasivoCorrienteId, Codigo = "2.1.6", Nombre = "Anticipos de clientes", Tipo = "Pasivo", Naturaleza = "Acreedora", EsImputable = true,  EsSistema = false, Estado = "Activa" },

            // NIVEL 3 — Pasivo No Corriente (2.2.X)
            new CuentaContable { Id = PrestamosBancariosLPId, PlanCuentasId = PlanTemplateId, CuentaPadreId = PasivoNoCorrienteId, Codigo = "2.2.1", Nombre = "Préstamos bancarios LP", Tipo = "Pasivo", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = OtrasDeudasLPId,        PlanCuentasId = PlanTemplateId, CuentaPadreId = PasivoNoCorrienteId, Codigo = "2.2.2", Nombre = "Otras deudas LP",        Tipo = "Pasivo", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },

            // NIVEL 3 — Capital (3.1.X)
            new CuentaContable { Id = CapitalSocialId,       PlanCuentasId = PlanTemplateId, CuentaPadreId = CapitalId, Codigo = "3.1.1", Nombre = "Capital social",        Tipo = "Patrimonio", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = AportesIrrevocablesId, PlanCuentasId = PlanTemplateId, CuentaPadreId = CapitalId, Codigo = "3.1.2", Nombre = "Aportes irrevocables",  Tipo = "Patrimonio", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },

            // NIVEL 3 — Resultados (3.2.X)
            new CuentaContable { Id = ResultadosAcumuladosId, PlanCuentasId = PlanTemplateId, CuentaPadreId = ResultadosId, Codigo = "3.2.1", Nombre = "Resultados acumulados",    Tipo = "Patrimonio", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = ResultadoEjercicioId,   PlanCuentasId = PlanTemplateId, CuentaPadreId = ResultadosId, Codigo = "3.2.2", Nombre = "Resultado del ejercicio",  Tipo = "Patrimonio", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },

            // NIVEL 3 — Ingresos Operativos (4.1.X)
            new CuentaContable { Id = VentasMercaderiaId,    PlanCuentasId = PlanTemplateId, CuentaPadreId = IngresosOperativosId, Codigo = "4.1.1", Nombre = "Ventas de mercadería",    Tipo = "Ingreso", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = VentasServiciosId,     PlanCuentasId = PlanTemplateId, CuentaPadreId = IngresosOperativosId, Codigo = "4.1.2", Nombre = "Ventas de servicios",     Tipo = "Ingreso", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = DescuentosObtenidosId, PlanCuentasId = PlanTemplateId, CuentaPadreId = IngresosOperativosId, Codigo = "4.1.3", Nombre = "Descuentos obtenidos",    Tipo = "Ingreso", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },

            // NIVEL 3 — Ingresos No Operativos (4.2.X)
            new CuentaContable { Id = InteresesGanadosId,       PlanCuentasId = PlanTemplateId, CuentaPadreId = IngresosNoOperativosId, Codigo = "4.2.1", Nombre = "Intereses ganados",          Tipo = "Ingreso", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = DiferenciaCambioGanadaId, PlanCuentasId = PlanTemplateId, CuentaPadreId = IngresosNoOperativosId, Codigo = "4.2.2", Nombre = "Diferencia de cambio ganada", Tipo = "Ingreso", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = OtrosIngresosId,          PlanCuentasId = PlanTemplateId, CuentaPadreId = IngresosNoOperativosId, Codigo = "4.2.3", Nombre = "Otros ingresos",             Tipo = "Ingreso", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },

            // NIVEL 3 — Costos (5.1.X)
            new CuentaContable { Id = CostoMercaderiaVendidaId, PlanCuentasId = PlanTemplateId, CuentaPadreId = CostosId, Codigo = "5.1.1", Nombre = "Costo de mercadería vendida", Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },

            // NIVEL 3 — Gastos de Personal (5.2.X)
            new CuentaContable { Id = SueldosJornalesId,     PlanCuentasId = PlanTemplateId, CuentaPadreId = GastosPersonalId, Codigo = "5.2.1", Nombre = "Sueldos y jornales",       Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = AportesPatronalesId,   PlanCuentasId = PlanTemplateId, CuentaPadreId = GastosPersonalId, Codigo = "5.2.2", Nombre = "Aportes patronales (BPS)",  Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = OtrosGastosPersonalId, PlanCuentasId = PlanTemplateId, CuentaPadreId = GastosPersonalId, Codigo = "5.2.3", Nombre = "Otros gastos de personal",  Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },

            // NIVEL 3 — Gastos Generales (5.3.X)
            new CuentaContable { Id = AlquilerId,                  PlanCuentasId = PlanTemplateId, CuentaPadreId = GastosGeneralesId, Codigo = "5.3.1", Nombre = "Alquiler",                     Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = ServiciosId,                 PlanCuentasId = PlanTemplateId, CuentaPadreId = GastosGeneralesId, Codigo = "5.3.2", Nombre = "Servicios (luz, agua, internet)", Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = PapeleriaUtilesId,           PlanCuentasId = PlanTemplateId, CuentaPadreId = GastosGeneralesId, Codigo = "5.3.3", Nombre = "Papelería y útiles de oficina", Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = MantenimientoReparacionesId, PlanCuentasId = PlanTemplateId, CuentaPadreId = GastosGeneralesId, Codigo = "5.3.4", Nombre = "Mantenimiento y reparaciones",   Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = SeguroId,                    PlanCuentasId = PlanTemplateId, CuentaPadreId = GastosGeneralesId, Codigo = "5.3.5", Nombre = "Seguros",                       Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },

            // NIVEL 3 — Gastos Financieros (5.4.X)
            new CuentaContable { Id = InteresesPagadosId,        PlanCuentasId = PlanTemplateId, CuentaPadreId = GastosFinancierosId, Codigo = "5.4.1", Nombre = "Intereses pagados",          Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = ComisionesBancariasId,     PlanCuentasId = PlanTemplateId, CuentaPadreId = GastosFinancierosId, Codigo = "5.4.2", Nombre = "Comisiones bancarias",        Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = DiferenciaCambioPerdidaId, PlanCuentasId = PlanTemplateId, CuentaPadreId = GastosFinancierosId, Codigo = "5.4.3", Nombre = "Diferencia de cambio perdida", Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },

            // NIVEL 3 — Depreciaciones (5.5.X)
            new CuentaContable { Id = DepreciacionBienesUsoId, PlanCuentasId = PlanTemplateId, CuentaPadreId = DepreciacionesId, Codigo = "5.5.1", Nombre = "Depreciación bienes de uso", Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },

            // NIVEL 3 — Impuestos (5.6.X)
            new CuentaContable { Id = IraeId,           PlanCuentasId = PlanTemplateId, CuentaPadreId = ImpuestosId, Codigo = "5.6.1", Nombre = "IRAE",              Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = OtrosImpuestosId, PlanCuentasId = PlanTemplateId, CuentaPadreId = ImpuestosId, Codigo = "5.6.2", Nombre = "Otros impuestos",   Tipo = "Egreso", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },

            // ─────────────────────────────────────────────────────────────
            // NIVEL 4 — Bancos (1.1.2.X)
            // ─────────────────────────────────────────────────────────────
            new CuentaContable { Id = BancoBROUId, PlanCuentasId = PlanTemplateId, CuentaPadreId = BancosId, Codigo = "1.1.2.1", Nombre = "BROU",    Tipo = "Activo", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = BancoITAUId, PlanCuentasId = PlanTemplateId, CuentaPadreId = BancosId, Codigo = "1.1.2.2", Nombre = "Itaú",    Tipo = "Activo", Naturaleza = "Deudora", EsImputable = true, EsSistema = false, Estado = "Activa" },

            // NIVEL 4 — Retenciones a pagar (2.1.4.X)
            new CuentaContable { Id = IrpfPagarId, PlanCuentasId = PlanTemplateId, CuentaPadreId = RetencionesPagarId, Codigo = "2.1.4.1", Nombre = "IRPF a pagar",   Tipo = "Pasivo", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" },
            new CuentaContable { Id = BpsPagarId,  PlanCuentasId = PlanTemplateId, CuentaPadreId = RetencionesPagarId, Codigo = "2.1.4.2", Nombre = "BPS a pagar",    Tipo = "Pasivo", Naturaleza = "Acreedora", EsImputable = true, EsSistema = false, Estado = "Activa" }
        );
    }
}
