using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public interface ILibroMayorPdfService
{
    byte[] Generar(LibroMayorViewModel vm);
}