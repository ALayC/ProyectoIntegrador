using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public interface IReporteCierreExcelService
{
    byte[] Generar(ReporteCierreViewModel vm);
}
