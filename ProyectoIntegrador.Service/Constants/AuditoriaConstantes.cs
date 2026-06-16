namespace ProyectoIntegrador.Service.Constants;

/// <summary>
/// Constantes para estandarizar los nombres de entidades y acciones en auditoría.
/// </summary>
public static class AuditoriaConstantes
{
    /// <summary>
    /// Nombres de entidades auditables.
    /// </summary>
    public static class Entidades
    {
        public const string AsientoContable = "AsientoContable";
        public const string Auditoria = "Auditoria";
        public const string CentroDeCosto = "CentroDeCosto";
        public const string Cliente = "Cliente";
        public const string Comprobante = "Comprobante";
        public const string CuentaContable = "CuentaContable";
        public const string EjercicioContable = "EjercicioContable";
        public const string Importacion = "Importacion";
        public const string LineaAsiento = "LineaAsiento";
        public const string Permiso = "Permiso";
        public const string PlanDeCuentas = "PlanDeCuentas";
        public const string Rol = "Rol";
        public const string RolPermiso = "RolPermiso";
        public const string SaldoCuenta = "SaldoCuenta";
        public const string TipoDeCambio = "TipoDeCambio";
        public const string TokenRevocado = "TokenRevocado";
        public const string Usuario = "Usuario";
    }

    /// <summary>
    /// Acciones auditables.
    /// </summary>
    public static class Acciones
    {
        public const string Crear = "Crear";
        public const string Editar = "Editar";
        public const string Activar = "Activar";
        public const string Desactivar = "Desactivar";
        public const string Anular = "Anular";
        public const string Cerrar = "Cerrar";
        public const string Confirmar = "Confirmar";
        public const string Revertir = "Revertir";
        public const string AsignarPermiso = "AsignarPermiso";
        public const string RemoverPermiso = "RemoverPermiso";
    }
}
