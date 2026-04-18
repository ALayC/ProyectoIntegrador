using System.Text.Json;
using ProyectoIntegrador.Data.Context;
using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPlanDeCuentasRepository _planDeCuentasRepository;
    private readonly IAuditoriaRepository _auditoriaRepository;

    public ClienteService(
IClienteRepository clienteRepository,
    IUsuarioRepository usuarioRepository,
        IPlanDeCuentasRepository planDeCuentasRepository,
        IAuditoriaRepository auditoriaRepository)
    {
   _clienteRepository = clienteRepository;
     _usuarioRepository = usuarioRepository;
    _planDeCuentasRepository = planDeCuentasRepository;
        _auditoriaRepository = auditoriaRepository;
    }

    /// <inheritdoc />
    public async Task<ClienteResponseDto> Crear(ClienteDto clienteDto, Guid contadorId)
    {
        // Validar RUT único
        var existeRut = await _clienteRepository.ExisteRut(clienteDto.Rut);
if (existeRut)
        {
      throw new DuplicadoException("RUT", clienteDto.Rut);
        }

     // Validar que el contadorId corresponda a un usuario con rol Contador
        var contador = await _usuarioRepository.ObtenerPorId(contadorId)
   ?? throw new EntidadNoEncontradaException("Usuario", contadorId);

        if (contador.Rol.Nombre != "Contador" && contador.Rol.Nombre != "Administrador")
        {
 throw new AccesoNoAutorizadoException(contadorId, "Crear Clientes");
        }

  // Crear el cliente
        var cliente = new Cliente
     {
       Id = Guid.NewGuid(),
ContadorId = contadorId,
            Rut = clienteDto.Rut,
      RazonSocial = clienteDto.RazonSocial,
 NombreFantasia = clienteDto.NombreFantasia,
            Email = clienteDto.Email,
         Telefono = clienteDto.Telefono,
            TipoContribuyente = clienteDto.TipoContribuyente,
            MonedaBase = clienteDto.MonedaBase,
    Estado = "Activo"
 };

        await _clienteRepository.Guardar(cliente);

        // Crear automáticamente el PlanDeCuentas asociado (relación 1:1)
        var planDeCuentas = new PlanDeCuentas
        {
            Id = Guid.NewGuid(),
         ClienteId = cliente.Id
     };

        await _planDeCuentasRepository.Guardar(planDeCuentas);

 // Registrar auditoría
        await RegistrarAuditoria(
       contadorId,
  "Cliente",
            "Crear",
        datosAnteriores: null,
  datosNuevos: SerializarCliente(cliente));

        return MapearAResponseDto(cliente);
    }

    /// <inheritdoc />
    public async Task<ClienteResponseDto> ObtenerPorId(Guid id)
    {
        var cliente = await _clienteRepository.ObtenerPorId(id)
        ?? throw new EntidadNoEncontradaException("Cliente", id);

    return MapearAResponseDto(cliente);
    }

    /// <inheritdoc />
    public async Task<PaginadoDto<ClienteResponseDto>> ObtenerPorContador(Guid contadorId, int pagina, int cantidadPorPagina)
    {
   var clientes = await _clienteRepository.ObtenerPorContador(contadorId, pagina, cantidadPorPagina);
var total = await _clienteRepository.ContarPorContador(contadorId);

        var clientesDto = clientes.Select(MapearAResponseDto).ToList();

    return new PaginadoDto<ClienteResponseDto>(clientesDto, pagina, cantidadPorPagina, total);
    }

    /// <inheritdoc />
  public async Task<ClienteResponseDto> Actualizar(Guid id, ClienteDto clienteDto, Guid usuarioId)
    {
        var cliente = await _clienteRepository.ObtenerPorId(id)
     ?? throw new EntidadNoEncontradaException("Cliente", id);

        // Si cambió el RUT, validar que no exista otro cliente con ese RUT
     if (cliente.Rut != clienteDto.Rut)
        {
      var existeRut = await _clienteRepository.ExisteRut(clienteDto.Rut);
 if (existeRut)
  {
       throw new DuplicadoException("RUT", clienteDto.Rut);
        }
        }

        // Capturar datos anteriores para auditoría
      var datosAnteriores = SerializarCliente(cliente);

   // Actualizar propiedades
        cliente.Rut = clienteDto.Rut;
        cliente.RazonSocial = clienteDto.RazonSocial;
   cliente.NombreFantasia = clienteDto.NombreFantasia;
        cliente.Email = clienteDto.Email;
        cliente.Telefono = clienteDto.Telefono;
      cliente.TipoContribuyente = clienteDto.TipoContribuyente;
        cliente.MonedaBase = clienteDto.MonedaBase;

        await _clienteRepository.Actualizar(cliente);

        // Registrar auditoría con datos anteriores y nuevos
        await RegistrarAuditoria(
        usuarioId,
            "Cliente",
     "Editar",
     datosAnteriores: datosAnteriores,
  datosNuevos: SerializarCliente(cliente));

     return MapearAResponseDto(cliente);
    }

    /// <inheritdoc />
    public async Task Desactivar(Guid id, Guid usuarioId)
    {
        var cliente = await _clienteRepository.ObtenerPorId(id)
   ?? throw new EntidadNoEncontradaException("Cliente", id);

        // Capturar datos anteriores
        var datosAnteriores = SerializarCliente(cliente);

        // Soft delete: cambiar estado a Inactivo
        cliente.Estado = "Inactivo";

        await _clienteRepository.Actualizar(cliente);

        // Registrar auditoría
      await RegistrarAuditoria(
            usuarioId,
    "Cliente",
          "Desactivar",
datosAnteriores: datosAnteriores,
            datosNuevos: SerializarCliente(cliente));
    }

    // ??????????????????????????????????????????????
    // Métodos privados
    // ??????????????????????????????????????????????

    private static ClienteResponseDto MapearAResponseDto(Cliente cliente)
    {
        return new ClienteResponseDto
     {
            Id = cliente.Id,
            ContadorId = cliente.ContadorId,
      Rut = cliente.Rut,
   RazonSocial = cliente.RazonSocial,
        NombreFantasia = cliente.NombreFantasia,
        Email = cliente.Email,
   Telefono = cliente.Telefono,
TipoContribuyente = cliente.TipoContribuyente,
          MonedaBase = cliente.MonedaBase,
      Estado = cliente.Estado
        };
    }

    private static string SerializarCliente(Cliente cliente)
    {
        return JsonSerializer.Serialize(new
        {
     cliente.Id,
         cliente.ContadorId,
cliente.Rut,
            cliente.RazonSocial,
         cliente.NombreFantasia,
  cliente.Email,
   cliente.Telefono,
    cliente.TipoContribuyente,
            cliente.MonedaBase,
  cliente.Estado
        });
    }

    private async Task RegistrarAuditoria(
        Guid usuarioId,
    string entidad,
        string accion,
        string? datosAnteriores,
        string? datosNuevos)
    {
        var auditoria = new Auditoria
        {
            Id = Guid.NewGuid(),
       UsuarioId = usuarioId,
          Entidad = entidad,
            Accion = accion,
   FechaHora = DateTime.UtcNow,
       DatosAnteriores = datosAnteriores,
            DatosNuevos = datosNuevos
  };

        await _auditoriaRepository.Guardar(auditoria);
    }
}
