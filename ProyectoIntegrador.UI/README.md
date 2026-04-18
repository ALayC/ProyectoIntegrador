# ProyectoIntegrador.UI

## Qué es esta capa

Es la interfaz web que usa el usuario (el contador y sus auxiliares). Muestra las pantallas, formularios y reportes del sistema. No contiene lógica de negocio; consume la API por HTTP para todas las operaciones.

## Qué contiene

- **Controllers/**: Controllers MVC que manejan la navegación y llaman a la API mediante HttpClient.
- **Views/**: Razor Views (.cshtml) organizadas por módulo funcional.
- **wwwroot/**: Archivos estáticos (CSS, JavaScript, imágenes, librerías como Bootstrap).
- **Program.cs**: Configuración de la aplicación y del HttpClient para consumir la API.

## Qué NO debe contener

- ❌ Lógica de negocio (eso va en ProyectoIntegrador.Service)
- ❌ Acceso directo a base de datos
- ❌ Referencia a ningún otro proyecto de la solución (se comunica solo por HTTP)
- ❌ Validaciones de reglas de negocio complejas (solo validaciones de frontend como campos obligatorios)

## Cómo consumir la API desde la UI

```csharp
// En un Controller de la UI
public class ClientesController : Controller
{
    private readonly HttpClient _httpClient;

    public ClientesController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("API");
    }

    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync("/api/clientes?pagina=1&cantidadPorPagina=20");
        
        if (response.IsSuccessStatusCode)
        {
            var clientes = await response.Content.ReadFromJsonAsync<PaginadoResponse<ClienteViewModel>>();
            return View(clientes);
        }

        // Manejar error
        return View("Error");
    }

    [HttpPost]
    public async Task<IActionResult> Crear(ClienteViewModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/clientes", model);

        if (response.IsSuccessStatusCode)
            return RedirectToAction("Index");

        // Leer el error de la API y mostrarlo al usuario
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        ModelState.AddModelError("", error.Error);
        return View(model);
    }
}
```

## Configuración del HttpClient

En `Program.cs`:

```csharp
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:PUERTO_DE_LA_API");
});
```

## Organización de Views

```
Views/
├── Shared/
│   ├── _Layout.cshtml          → Layout principal (menú, header, footer)
│   └── _ValidationScripts.cshtml
├── Home/
│   └── Index.cshtml            → Dashboard principal
├── Auth/
│   ├── Login.cshtml
│   └── Register.cshtml
├── Clientes/
│   ├── Index.cshtml            → Listado de clientes
│   ├── Crear.cshtml            → Formulario de alta
│   └── Editar.cshtml           → Formulario de edición
├── Asientos/
│   ├── Index.cshtml            → Libro diario
│   └── Crear.cshtml            → Formulario de asiento
└── Reportes/
    ├── LibroMayor.cshtml
    ├── BalanceGeneral.cshtml
    ├── EstadoResultados.cshtml
    └── ResumenIVA.cshtml
```

## Referencia a otros proyectos

- ❌ No referencia a ningún otro proyecto
- Se comunica con ProyectoIntegrador.API exclusivamente por HTTP
