namespace ProyectoIntegrador.Service.DTOs
{
    /// <summary>
    /// Filtros de consulta para generar el Balance General.
    /// </summary>
    public class BalanceGeneralFiltroDto
    {
        public Guid ClienteId { get; set; }

        public DateOnly FechaHasta { get; set; }
    }

    /// <summary>
    /// Nodo del árbol del Balance General.
    /// Representa una cuenta contable y sus saldos acumulados.
    /// </summary>
    public class BalanceGeneralNodoDto
    {
        public Guid CuentaId { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public decimal Saldo { get; set; }

        public List<BalanceGeneralNodoDto> Hijas { get; set; } = new();
    }

    /// <summary>
    /// Respuesta del Balance General para un cliente a una fecha determinada.
    /// </summary>
    public class BalanceGeneralResponseDto
    {
        public decimal TotalActivo { get; set; }

        public decimal TotalPasivo { get; set; }

        public decimal TotalPatrimonio { get; set; }

        public decimal TotalPasivoPatrimonio { get; set; }

        public bool Balancea { get; set; }

        public List<BalanceGeneralNodoDto> Activos { get; set; } = new();

        public List<BalanceGeneralNodoDto> Pasivos { get; set; } = new();

        public List<BalanceGeneralNodoDto> Patrimonio { get; set; } = new();
    }
}