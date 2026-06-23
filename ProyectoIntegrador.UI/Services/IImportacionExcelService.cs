using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public interface IImportacionExcelService
{
    /// <summary>
    /// Genera un archivo Excel template personalizado con las cuentas imputables del cliente
    /// en una hoja oculta y un dropdown en la columna CodigoCuenta.
    /// </summary>
    byte[] GenerarTemplate(string clienteNombre, List<CuentaContableViewModel> cuentas);

    /// <summary>
    /// Parsea el archivo Excel subido por el usuario.
    /// Lanza <see cref="ImportacionFormatoException"/> si la estructura es inválida.
    /// Retorna los asientos con sus errores de validación inline.
    /// </summary>
    List<AsientoImportacionViewModel> Parsear(
        Stream archivoExcel,
        Dictionary<string, CuentaContableViewModel> cuentasPorCodigo);
}
