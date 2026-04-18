# ProyectoIntegrador.API

## Qué es esta capa

Es la capa que expone los endpoints HTTP del sistema. Recibe las solicitudes de la UI, las valida, delega la lógica al servicio correspondiente y devuelve la respuesta.

## Qué contiene

- **Controllers/**: Endpoints HTTP organizados por entidad o dominio. Cada controller es deliberadamente delgado: recibe la request, llama al servicio, retorna la response.
- **Middleware/**: Middleware global de excepciones que traduce excepciones de dominio a códigos HTTP estándar.
- **Program.cs**: Configuración de la aplicación: inyección de dependencias, JWT, rate limiting, middleware.

## Qué NO debe contener

- ❌ Lógica de negocio (eso va en ProyectoIntegrador.Service)
- ❌ Acceso directo a base de datos o DbContext (eso va en ProyectoIntegrador.Data)
- ❌ Validaciones de reglas de negocio complejas (solo validaciones de formato de entrada)

## Cómo crear un nuevo Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    // Inyección por constructor, siempre con interfaz
    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpPost]
    public async Task<IActionResult> Crear(ClienteDto dto)
    {
        // El controller NO valida reglas de negocio
        // Solo llama al servicio y retorna el resultado
        var resultado = await _clienteService.Crear(dto);
        return CreatedAtAction(nameof(Crear), resultado);
    }
}
```

## Referencia a otros proyectos

- ✅ ProyectoIntegrador.Service
- ✅ ProyectoIntegrador.Data
