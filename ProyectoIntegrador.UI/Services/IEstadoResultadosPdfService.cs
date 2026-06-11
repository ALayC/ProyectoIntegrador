using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public interface IEstadoResultadosPdfService
{
    byte[] Generar(EstadoResultadosViewModel vm);
}