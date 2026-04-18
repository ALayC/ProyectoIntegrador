using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface IClienteService
{
    /// <summary>
    /// Crea un nuevo cliente asociado al contador indicado.
    /// Valida RUT único, crea PlanDeCuentas y registra auditoría.
 /// </summary>
    Task<ClienteResponseDto> Crear(ClienteDto clienteDto, Guid contadorId);

    /// <summary>
    /// Obtiene un cliente por su Id.
  /// Lanza EntidadNoEncontradaException si no existe.
    /// </summary>
    Task<ClienteResponseDto> ObtenerPorId(Guid id);

    /// <summary>
    /// Obtiene los clientes de un contador con paginación.
    /// </summary>
    Task<PaginadoDto<ClienteResponseDto>> ObtenerPorContador(Guid contadorId, int pagina, int cantidadPorPagina);

    /// <summary>
    /// Actualiza los datos de un cliente existente.
    /// Registra auditoría con datos anteriores y nuevos.
    /// </summary>
    Task<ClienteResponseDto> Actualizar(Guid id, ClienteDto clienteDto, Guid usuarioId);

    /// <summary>
    /// Desactiva un cliente (soft delete).
    /// Cambia estado a Inactivo y registra auditoría.
    /// </summary>
    Task Desactivar(Guid id, Guid usuarioId);
}
