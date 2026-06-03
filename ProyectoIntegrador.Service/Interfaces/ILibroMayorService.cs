using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface ILibroMayorService
{
    /// <summary>
    /// Genera el Libro Mayor para un cliente con filtros opcionales de cuenta, fechas y ejercicio.
    /// </summary>
    Task<LibroMayorResponseDto> Obtener(LibroMayorFiltroDto filtro);
}
