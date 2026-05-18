using ProyectoIntegrador.Data.Entities;
using ProyectoIntegrador.Data.Repositories.Interfaces;
using ProyectoIntegrador.Service.Constants;
using ProyectoIntegrador.Service.DTOs;
using ProyectoIntegrador.Service.Exceptions;
using ProyectoIntegrador.Service.Interfaces;

namespace ProyectoIntegrador.Service.Implementations;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPlanDeCuentasRepository _planDeCuentasRepository;
    private readonly IAuditoriaService _auditoriaService;
    private readonly ICuentaContableService _cuentaContableService;

    public ClienteService(
        IClienteRepository clienteRepository,
        IUsuarioRepository usuarioRepository,
        IPlanDeCuentasRepository planDeCuentasRepository,
        IAuditoriaService auditoriaService,
        ICuentaContableService cuentaContableService)
    {
        _clienteRepository = clienteRepository;
        _usuarioRepository = usuarioRepository;
        _planDeCuentasRepository = planDeCuentasRepository;
        _auditoriaService = auditoriaService;
        _cuentaContableService = cuentaContableService;
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
            ClienteId = cliente.Id,
            EsTemplate = false
        };

        await ClonarPlanDeCuentas(planDeCuentas);
        await _planDeCuentasRepository.Guardar(planDeCuentas);

        // Registrar auditoría usando el servicio centralizado.
        await _auditoriaService.Registrar(
            contadorId,
            AuditoriaConstantes.Entidades.Cliente,
            AuditoriaConstantes.Acciones.Crear,
            datosAnteriores: null,
            datosNuevos: ConstruirDatosAuditoria(cliente));

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
        var datosAnteriores = ConstruirDatosAuditoria(cliente);

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
        await _auditoriaService.Registrar(
            usuarioId,
            AuditoriaConstantes.Entidades.Cliente,
            AuditoriaConstantes.Acciones.Editar,
            datosAnteriores: datosAnteriores,
            datosNuevos: ConstruirDatosAuditoria(cliente));

        return MapearAResponseDto(cliente);
    }

    /// <inheritdoc />
    public async Task Desactivar(Guid id, Guid usuarioId)
    {
        var cliente = await _clienteRepository.ObtenerPorId(id)
   ?? throw new EntidadNoEncontradaException("Cliente", id);

        // Capturar datos anteriores
        var datosAnteriores = ConstruirDatosAuditoria(cliente);

        // Soft delete: cambiar estado a Inactivo
        cliente.Estado = "Inactivo";

        await _clienteRepository.Actualizar(cliente);

        // Registrar auditoría
        await _auditoriaService.Registrar(
            usuarioId,
            AuditoriaConstantes.Entidades.Cliente,
            AuditoriaConstantes.Acciones.Desactivar,
            datosAnteriores: datosAnteriores,
            datosNuevos: ConstruirDatosAuditoria(cliente));
    }

    /// <inheritdoc />
    public async Task Activar(Guid id, Guid usuarioId)
    {
        var cliente = await _clienteRepository.ObtenerPorId(id)
            ?? throw new EntidadNoEncontradaException("Cliente", id);

        var datosAnteriores = ConstruirDatosAuditoria(cliente);

        cliente.Estado = "Activo";

        await _clienteRepository.Actualizar(cliente);

        await _auditoriaService.Registrar(
            usuarioId,
            AuditoriaConstantes.Entidades.Cliente,
            AuditoriaConstantes.Acciones.Activar,
            datosAnteriores: datosAnteriores,
            datosNuevos: ConstruirDatosAuditoria(cliente));
    }

    /// <inheritdoc />
    public async Task<List<CuentaContableArbolDto>> ObtenerPlanDeCuentas(Guid clienteId)
    {
        var plan = await _planDeCuentasRepository.ObtenerPorClienteId(clienteId)
            ?? throw new EntidadNoEncontradaException("PlanDeCuentas", clienteId);

        return await _cuentaContableService.ObtenerArbolDeCuentas(plan.Id);
    }

    /// <inheritdoc />
    public async Task<List<CuentaContableDto>> ObtenerCuentasImputables(Guid clienteId)
    {
        var plan = await _planDeCuentasRepository.ObtenerPorClienteId(clienteId)
            ?? throw new EntidadNoEncontradaException("PlanDeCuentas", clienteId);

        return await _cuentaContableService.ObtenerImputables(plan.Id);
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

    /// <summary>
    /// Construye el objeto base para auditar cambios de cliente.
    /// </summary>
    private static object ConstruirDatosAuditoria(Cliente cliente)
    {
        return new
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
        };
    }

    private async Task ClonarPlanDeCuentas(PlanDeCuentas destino)
    {
        var template = await _planDeCuentasRepository.ObtenerTemplate();
        if (template is null)
        {
            throw new InvalidOperationException("No se encontró el plan de cuentas template. Verificá la configuración/seed del sistema.");
        }

        var cuentasClonadas = new List<CuentaContable>();
        var mapping = new Dictionary<Guid, CuentaContable>();

        foreach (var cuentaTemplate in template.CuentasContables)
        {
            var cuentaNueva = new CuentaContable
            {
                Id = Guid.NewGuid(),
                PlanCuentasId = destino.Id,
                CuentaPadreId = null,
                Codigo = cuentaTemplate.Codigo,
                Nombre = cuentaTemplate.Nombre,
                Tipo = cuentaTemplate.Tipo,
                Naturaleza = cuentaTemplate.Naturaleza,
                EsImputable = cuentaTemplate.EsImputable,
                EsSistema = cuentaTemplate.EsSistema,
                Estado = cuentaTemplate.Estado
            };

            cuentasClonadas.Add(cuentaNueva);
            mapping[cuentaTemplate.Id] = cuentaNueva;
        }

        foreach (var cuentaTemplate in template.CuentasContables)
        {
            if (cuentaTemplate.CuentaPadreId is null)
            {
                continue;
            }

            var cuentaNueva = mapping[cuentaTemplate.Id];
            cuentaNueva.CuentaPadreId = mapping[cuentaTemplate.CuentaPadreId.Value].Id;
        }

        destino.CuentasContables = cuentasClonadas;
    }
}