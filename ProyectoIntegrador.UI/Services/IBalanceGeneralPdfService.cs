using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public interface IBalanceGeneralPdfService
{
    byte[] Generar(BalanceGeneralViewModel vm);
}
