using Microsoft.EntityFrameworkCore;
using ProyectoIntegrador.Data.Entities;

namespace ProyectoIntegrador.Data.Context;

/// <summary>
/// Seed data de roles, permisos y asignaciones predefinidas.
/// Todos los IDs son Guids fijos para consistencia entre ambientes.
/// </summary>
public static class SeedData
{
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

    // ──────────────────────────────────────────────
    // Método principal de seed
    // ──────────────────────────────────────────────
    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedRoles(modelBuilder);
        SeedPermisos(modelBuilder);
        SeedRolPermisos(modelBuilder);
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
        // Todos los IDs de permisos
        var todosLosPermisos = new[]
        {
            // Usuarios
     PermUsuariosCrear, PermUsuariosConsultar, PermUsuariosEditar, PermUsuariosDesactivar,
  // Clientes
          PermClientesCrear, PermClientesConsultar, PermClientesEditar, PermClientesDesactivar,
          // Cuentas
            PermCuentasCrear, PermCuentasConsultar, PermCuentasEditar, PermCuentasDesactivar,
      // Asientos
       PermAsientosCrear, PermAsientosConsultar, PermAsientosRevertir,
   // Comprobantes
            PermComprobantesCrear, PermComprobantesConsultar, PermComprobantesEditar,
   // Importaciones
            PermImportacionesCrear, PermImportacionesConsultar,
    // Reportes
       PermReportesConsultar, PermReportesExportar,
            // Ejercicios
   PermEjerciciosCrear, PermEjerciciosConsultar, PermEjerciciosEditar, PermEjerciciosDesactivar,
      // CentrosCosto
      PermCentrosCostoCrear, PermCentrosCostoConsultar, PermCentrosCostoEditar, PermCentrosCostoDesactivar
        };

        // Permisos del módulo Usuarios (excluidos para Contador)
        var permisosUsuarios = new[]
              {
PermUsuariosCrear, PermUsuariosConsultar, PermUsuariosEditar, PermUsuariosDesactivar
        };

        // Permisos del Contador: todos excepto el módulo Usuarios
        var permisosContador = todosLosPermisos
            .Except(permisosUsuarios)
.ToArray();

        // Permisos del Auxiliar Contable: Consultar en todos + Crear en Asientos y Comprobantes
        var permisosAuxiliar = new[]
      {
 // Consultar en todos los módulos
          PermUsuariosConsultar,
 PermClientesConsultar,
      PermCuentasConsultar,
     PermAsientosConsultar,
   PermComprobantesConsultar,
        PermImportacionesConsultar,
     PermReportesConsultar,
     PermEjerciciosConsultar,
            PermCentrosCostoConsultar,
            // Crear en Asientos y Comprobantes
     PermAsientosCrear,
            PermComprobantesCrear
        };

        // ── Administrador: todos los permisos ──
        var rolPermisosAdmin = todosLosPermisos
       .Select(permisoId => new RolPermiso
       {
           RolId = RolAdministradorId,
           PermisoId = permisoId
       });

        // ── Contador: todos excepto Usuarios ──
        var rolPermisosContador = permisosContador
            .Select(permisoId => new RolPermiso
            {
                RolId = RolContadorId,
                PermisoId = permisoId
            });

        // ── Auxiliar Contable: consultar + crear asientos/comprobantes ──
        var rolPermisosAuxiliar = permisosAuxiliar
      .Select(permisoId => new RolPermiso
      {
          RolId = RolAuxiliarId,
          PermisoId = permisoId
      });

        modelBuilder.Entity<RolPermiso>().HasData(
    rolPermisosAdmin
    .Concat(rolPermisosContador)
.Concat(rolPermisosAuxiliar)
      .ToArray()
  );
    }
}
