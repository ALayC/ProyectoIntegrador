using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public interface IEstadoResultadosExcelService
{
    byte[] Generar(EstadoResultadosViewModel vm);
}