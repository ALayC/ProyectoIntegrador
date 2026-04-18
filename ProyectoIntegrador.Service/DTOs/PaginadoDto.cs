namespace ProyectoIntegrador.Service.DTOs;

/// <summary>
/// DTO genérico para respuestas paginadas.
/// Formato estándar: { datos, pagina, cantidadPorPagina, totalRegistros, totalPaginas }
/// </summary>
public class PaginadoDto<T>
{
    public List<T> Datos { get; set; } = new();
    public int Pagina { get; set; }
    public int CantidadPorPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }

    public PaginadoDto(List<T> datos, int pagina, int cantidadPorPagina, int totalRegistros)
    {
   Datos = datos;
     Pagina = pagina;
   CantidadPorPagina = cantidadPorPagina;
     TotalRegistros = totalRegistros;
        TotalPaginas = (int)Math.Ceiling((double)totalRegistros / cantidadPorPagina);
    }
}
