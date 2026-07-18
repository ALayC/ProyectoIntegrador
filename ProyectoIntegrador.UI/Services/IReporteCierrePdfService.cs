using ProyectoIntegrador.UI.Models;

namespace ProyectoIntegrador.UI.Services;

public interface IReporteCierrePdfService
{
    byte[] Generar(ReporteCierreViewModel vm);
}
