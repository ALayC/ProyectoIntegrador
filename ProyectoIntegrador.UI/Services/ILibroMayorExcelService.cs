using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public interface ILibroMayorExcelService
{
    byte[] Generar(LibroMayorViewModel vm);
}