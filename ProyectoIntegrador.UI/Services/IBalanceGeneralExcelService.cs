using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public interface IBalanceGeneralExcelService
{
    byte[] Generar(BalanceGeneralViewModel vm);
}
