# ProyectoIntegrador.Data

## Qué es esta capa

Es la capa que gestiona el acceso a la base de datos. Define las entidades del dominio, las interfaces de repositorio, sus implementaciones y el DbContext de Entity Framework Core.

## Qué contiene

- **Entities/**: Las clases que representan las tablas de la base de datos (AsientoContable, Cliente, CuentaContable, etc.).
- **Repositories/Interfaces/**: Interfaces de repositorio que definen las operaciones de acceso a datos disponibles (IAsientoRepository, IClienteRepository, etc.).
- **Repositories/Implementations/**: Implementaciones concretas de los repositorios usando Entity Framework Core.
- **Context/**: El AppDbContext que configura Entity Framework Core y mapea las entidades a tablas.
- **Migrations/**: Migraciones de Entity Framework Core para crear y actualizar la base de datos.

## Qué NO debe contener

- ❌ Lógica de negocio (eso va en ProyectoIntegrador.Service)
- ❌ Controllers ni endpoints HTTP
- ❌ DTOs (eso va en Service)
- ❌ Validaciones de reglas de negocio

## Cómo crear una nueva Entidad

```csharp
// En Entities/
public class Cliente
{
    public Guid Id { get; set; }
    public Guid ContadorId { get; set; }
    public string Rut { get; set; }
    public string RazonSocial { get; set; }
    public string? NombreFantasia { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public string TipoContribuyente { get; set; }
    public string MonedaBase { get; set; }
    public string Estado { get; set; }

    // Navegación
    public Usuario Contador { get; set; }
    public PlanDeCuentas PlanDeCuentas { get; set; }
    public ICollection<AsientoContable> Asientos { get; set; }
}
```

Luego registrar en el DbContext:

```csharp
// En Context/AppDbContext.cs
public DbSet<Cliente> Clientes { get; set; }
```

## Cómo crear un nuevo Repositorio

1. Crear la interfaz en `Repositories/Interfaces/`:

```csharp
public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorId(Guid id);
    Task<Cliente?> ObtenerPorRut(string rut);
    Task<List<Cliente>> ObtenerPorContador(Guid contadorId, int pagina, int cantidad);
    Task Guardar(Cliente cliente);
    Task Actualizar(Cliente cliente);
    Task<int> ContarPorContador(Guid contadorId);
}
```

2. Crear la implementación en `Repositories/Implementations/`:

```csharp
public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> ObtenerPorId(Guid id)
    {
        return await _context.Clientes.FindAsync(id);
    }

    public async Task Guardar(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
    }
}
```

3. Registrar en la inyección de dependencias (en Program.cs de API):

```csharp
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
```

## Migraciones

Para crear una migración después de modificar entidades o el DbContext:

```bash
dotnet ef migrations add NombreDescriptivo --project ProyectoIntegrador.Data --startup-project ProyectoIntegrador.API
```

Para aplicar la migración a la base de datos:

```bash
dotnet ef database update --project ProyectoIntegrador.Data --startup-project ProyectoIntegrador.API
```

## Referencia a otros proyectos

- ❌ No referencia a ningún otro proyecto (es la capa más baja)
