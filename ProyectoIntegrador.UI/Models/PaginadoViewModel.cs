namespace ProyectoIntegrador.UI.Models;

public class PaginadoViewModel<T>
{
public List<T> Datos { get; set; } = new();
    public int Pagina { get; set; }
    public int CantidadPorPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }

    public bool TienePaginaAnterior => Pagina > 1;
    public bool TienePaginaSiguiente => Pagina < TotalPaginas;
}
