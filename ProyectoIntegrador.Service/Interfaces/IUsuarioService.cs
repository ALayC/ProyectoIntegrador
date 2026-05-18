using ProyectoIntegrador.Service.DTOs;

namespace ProyectoIntegrador.Service.Interfaces;

public interface IUsuarioService
{
    Task<List<UsuarioResponseDto>> ObtenerTodos();
    Task<UsuarioResponseDto> ObtenerPorId(Guid id);
    Task<UsuarioResponseDto> Crear(CrearUsuarioDto dto, Guid adminId);
    Task<UsuarioResponseDto> Editar(Guid id, EditarUsuarioDto dto, Guid adminId);
    Task Desactivar(Guid id, Guid adminId);
}
