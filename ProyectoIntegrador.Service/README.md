# ProyectoIntegrador.Service

## Qué es esta capa

Es la capa que contiene toda la lógica de negocio del sistema. Aquí se validan las reglas contables, se orquestan las operaciones y se decide qué puede y qué no puede hacer el sistema.

## Qué contiene

- **Interfaces/**: Interfaces de cada servicio (IAsientoService, IClienteService, etc.). Los controllers y los tests dependen de estas interfaces, nunca de las implementaciones concretas.
- **Implementations/**: Implementaciones de los servicios con la lógica de negocio real.
- **DTOs/**: Objetos de transferencia de datos. Son los objetos que entran y salen de los servicios. No se exponen las entidades de Data directamente.
- **Exceptions/**: Excepciones de dominio tipadas que representan violaciones de reglas de negocio.

## Qué NO debe contener

- ❌ Controllers ni nada relacionado con HTTP
- ❌ DbContext ni consultas SQL directas (se accede a datos solo mediante interfaces de repositorio)
- ❌ Razor Views, HTML, CSS

## Cómo crear un nuevo Servicio

1. Crear la interfaz en `Interfaces/`:

```csharp
public interface IClienteService
{
    Task<ClienteDto> Crear(ClienteDto dto);
    Task<ClienteDto> ObtenerPorId(Guid id);
    Task Desactivar(Guid id);
}
```

2. Crear la implementación en `Implementations/`:

```csharp
public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepo;
    private readonly IAuditoriaRepository _auditoriaRepo;

    // Inyección por constructor con interfaces de repositorio
    public ClienteService(IClienteRepository clienteRepo, IAuditoriaRepository auditoriaRepo)
    {
        _clienteRepo = clienteRepo;
        _auditoriaRepo = auditoriaRepo;
    }

    public async Task<ClienteDto> Crear(ClienteDto dto)
    {
        // Validar reglas de negocio ACÁ
        var existente = await _clienteRepo.ObtenerPorRut(dto.Rut);
        if (existente != null)
            throw new DuplicadoException("Ya existe un cliente con ese RUT");

        // Crear entidad y guardar
        var cliente = new Cliente { /* mapear desde dto */ };
        await _clienteRepo.Guardar(cliente);

        // Registrar auditoría
        await _auditoriaRepo.Registrar(/* ... */);

        return MapearADto(cliente);
    }
}
```

3. Registrar en la inyección de dependencias (en Program.cs de API):

```csharp
builder.Services.AddScoped<IClienteService, ClienteService>();
```

## Cómo crear una Excepción de dominio

```csharp
// En Exceptions/
public class AsientoNoBalanceadoException : Exception
{
    public AsientoNoBalanceadoException()
        : base("El asiento no está balanceado: la suma del debe no es igual a la suma del haber") { }
}
```

El middleware de la API la traduce automáticamente a HTTP 400.

## Referencia a otros proyectos

- ✅ ProyectoIntegrador.Data
- ❌ No referencia a API ni a UI
