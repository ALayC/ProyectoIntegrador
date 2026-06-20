# Arquitectura del Sistema

## Resumen

El sistema adopta una arquitectura en capas con el patrón Repository + Service Layer. La comunicación entre capas es unidireccional descendente:

```
UI  ──(HTTP)──▶  API  ──▶  Service  ──▶  Data  ──▶  SQL Server
```

Cada capa tiene responsabilidades claras y se comunica con la siguiente a través de interfaces. Nunca se salta una capa.

---

## Reglas generales (LEER ANTES DE ESCRIBIR CÓDIGO)

### Lo que NUNCA se debe hacer

- **NUNCA** escribir lógica de negocio en un Controller. Los controllers solo reciben la request, llaman al servicio y devuelven la response.
- **NUNCA** usar el DbContext fuera de la capa Data. Solo los repositorios acceden a la base de datos.
- **NUNCA** referenciar ProyectoIntegrador.API o ProyectoIntegrador.UI desde Service o Data.
- **NUNCA** hacer llamadas HTTP desde Service o Data. Solo la UI habla por HTTP con la API.
- **NUNCA** commitear a main directamente. Siempre crear rama y abrir PR.
- **NUNCA** mergear un PR sin que los tests pasen y sin aprobación de otro integrante.
- **NUNCA** eliminar registros de la base de datos. Se usa desactivación (soft delete) o reversión.

### Lo que SIEMPRE se debe hacer

- **SIEMPRE** inyectar dependencias por constructor usando interfaces (ej. `IAsientoRepository`, no `AsientoRepository`).
- **SIEMPRE** crear tests unitarios al agregar un servicio nuevo (mínimo 2: happy path + caso de error).
- **SIEMPRE** validar reglas de negocio en la capa Service, no en el Controller ni en el Repository.
- **SIEMPRE** lanzar excepciones de dominio tipadas cuando una regla de negocio falla (ej. `AsientoNoBalanceadoException`).
- **SIEMPRE** registrar operaciones sensibles en la tabla de Auditoría.

---

## Capas del sistema

### ProyectoIntegrador.API

**Qué es:** Expone los endpoints HTTP que consume la UI.

**Qué contiene:**
- Controllers (delgados: reciben request → llaman servicio → devuelven response)
- Configuración de JWT y OAuth 2.0
- Middleware global de excepciones (`ExceptionMiddleware`)
- Middleware de logging de requests HTTP (`RequestLoggingMiddleware`)
- Rate limiting
- Configuración de inyección de dependencias

**Qué NO contiene:**
- Lógica de negocio
- Acceso directo a base de datos
- Validaciones de reglas de negocio (eso va en Service)

**Referencia a:** Service, Data

### ProyectoIntegrador.Service

**Qué es:** Contiene toda la lógica de negocio.

**Qué contiene:**
- Servicios de dominio (AsientoService, ClienteService, IVAService, etc.)
- Interfaces de servicios (IAsientoService, IClienteService, etc.)
- Excepciones de dominio tipadas (AsientoNoBalanceadoException, etc.)
- DTOs (objetos de transferencia de datos)

**Qué NO contiene:**
- Controllers ni nada relacionado con HTTP
- DbContext ni acceso directo a base de datos
- Razor Views ni HTML

**Referencia a:** Data

### ProyectoIntegrador.Data

**Qué es:** Gestiona el acceso a la base de datos.

**Qué contiene:**
- Entidades del dominio (AsientoContable.cs, Cliente.cs, etc.)
- Interfaces de repositorio (IAsientoRepository, IClienteRepository, etc.)
- Implementaciones de repositorio (AsientoRepository, ClienteRepository, etc.)
- DbContext (Entity Framework Core)
- Migraciones de base de datos

**Qué NO contiene:**
- Lógica de negocio (eso va en Service)
- Controllers ni endpoints
- Nada que dependa de HTTP

**Referencia a:** Ningún otro proyecto

### ProyectoIntegrador.UI

**Qué es:** La interfaz web que usa el usuario.

**Qué contiene:**
- Razor Views (.cshtml)
- HTML, CSS, JavaScript
- Bootstrap
- HttpClient para consumir la API

**Qué NO contiene:**
- Lógica de negocio
- Acceso a base de datos
- No referencia a ningún otro proyecto de la solución

**Referencia a:** Ningún otro proyecto (se comunica con la API por HTTP)

### ProyectoIntegrador.Test

**Qué es:** Tests unitarios de la capa de servicios.

**Qué contiene:**
- Clases de test por servicio (AsientoServiceTests.cs, etc.)
- Mocks de repositorios usando Moq

**Referencia a:** Service, Data

---

## Convenciones de nomenclatura

### Código C#

| Elemento | Convención | Ejemplo |
|----------|-----------|---------|
| Clases | PascalCase | `AsientoService`, `CuentaContable` |
| Interfaces | I + PascalCase | `IAsientoService`, `IAsientoRepository` |
| Métodos | PascalCase | `ValidarBalance()`, `CrearAsiento()` |
| Propiedades | PascalCase | `RazonSocial`, `FechaInicio` |
| Variables locales | camelCase | `importeTotal`, `clienteId` |
| Parámetros | camelCase | `decimal debe`, `Guid ejercicioId` |
| Constantes | PascalCase | `MaxRegistrosPorPagina` |
| Archivos | Mismo nombre que la clase | `AsientoService.cs` |

### Tests

Los métodos de test siguen el patrón: `MetodoQueTesteas_Escenario_ResultadoEsperado`

Ejemplos:
```csharp
CrearAsiento_ConDebeIgualAHaber_GuardaExitosamente()
CrearAsiento_ConDebeDistintoDeHaber_LanzaAsientoNoBalanceadoException()
CerrarEjercicio_ConEjercicioYaCerrado_LanzaEjercicioCerradoException()
```

### Ramas de Git

| Prefijo | Uso | Ejemplo |
|---------|-----|---------|
| feature/ | Nueva funcionalidad | feature/importacion-excel |
| fix/ | Corrección de bug | fix/balance-asiento |
| refactor/ | Mejora sin cambio funcional | refactor/asiento-service |
| docs/ | Cambios en documentación | docs/plan-calidad |

### Commits

Mensajes descriptivos en español:
- "Agrega validación de balance en AsientoService"
- "Corrige cálculo de IVA para tasa mínima"
- "Agrega tests unitarios para CierreEjercicioService"

---

## Estructura de carpetas por proyecto

```
ProyectoIntegrador.API/
├── Controllers/
│   ├── AsientosController.cs
│   ├── ClientesController.cs
│   ├── AuthController.cs
│   └── ReportesController.cs
├── Middleware/
│   ├── ExceptionMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Program.cs
└── appsettings.json

ProyectoIntegrador.Service/
├── Interfaces/
│   ├── IAsientoService.cs
│   ├── IClienteService.cs
│   └── ...
├── Implementations/
│   ├── AsientoService.cs
│   ├── ClienteService.cs
│   └── ...
├── DTOs/
│   ├── AsientoDto.cs
│   ├── ClienteDto.cs
│   └── ...
└── Exceptions/
    ├── AsientoNoBalanceadoException.cs
    ├── EjercicioCerradoException.cs
    └── ...

ProyectoIntegrador.Data/
├── Entities/
│   ├── AsientoContable.cs
│   ├── Cliente.cs
│   ├── CuentaContable.cs
│   └── ...
├── Repositories/
│   ├── Interfaces/
│   │   ├── IAsientoRepository.cs
│   │   └── ...
│   └── Implementations/
│       ├── AsientoRepository.cs
│       └── ...
├── Context/
│   └── AppDbContext.cs
└── Migrations/

ProyectoIntegrador.Test/
├── AsientoServiceTests.cs
├── ClienteServiceTests.cs
├── CierreEjercicioServiceTests.cs
└── ...

ProyectoIntegrador.UI/
├── Controllers/
│   └── HomeController.cs
├── Views/
│   ├── Shared/
│   ├── Asientos/
│   ├── Clientes/
│   └── ...
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
└── Program.cs
```

---

## Manejo de errores

Los servicios lanzan excepciones tipadas. El middleware global las traduce a HTTP:

| Excepción | Código HTTP | Cuándo |
|-----------|------------|--------|
| `AsientoNoBalanceadoException` | 400 | Debe ≠ Haber |
| `EjercicioCerradoException` | 400 | Operar en ejercicio cerrado |
| `CuentaNoImputableException` | 400 | Movimiento contra cuenta agrupadora |
| `EntidadNoEncontradaException` | 404 | Recurso inexistente |
| `AccesoNoAutorizadoException` | 403 | Sin permisos |
| `DuplicadoException` | 409 | RUT o email ya existente |
| `EjercicioSolapadoException` | 400 | Ejercicio se solapa con otro |
| `ImportacionInvalidaException` | 400 | Excel con estructura incorrecta |

---

## Logging y Observabilidad

El sistema implementa logging técnico en dos capas: el pipeline HTTP de la API y la capa de servicios. En producción, todos los logs se envían a **Azure Application Insights**. En desarrollo, se escriben en la consola.

### Configuración

La integración con Application Insights es condicional: solo se activa si `ApplicationInsights:ConnectionString` tiene valor en `appsettings.json`. Esto permite correr la aplicación localmente sin una cuenta de Azure.

```json
// appsettings.json (producción)
"ApplicationInsights": {
  "ConnectionString": "InstrumentationKey=...;IngestionEndpoint=..."
}

// appsettings.Development.json (local, sin App Insights)
"ApplicationInsights": {
  "ConnectionString": ""
}
```

Los niveles de log difieren por ambiente:

| Ambiente | Nivel por defecto | EF Core SQL |
|----------|-------------------|-------------|
| Development | `Debug` | `Information` (muestra queries) |
| Production | `Warning` | no configurado |

### Pipeline HTTP — `RequestLoggingMiddleware`

Registra **cada request HTTP** sin excepción, con método, ruta, status code, duración en ms, userId y dirección IP. El nivel varía según el status code:

| Status | Nivel | Ejemplo |
|--------|-------|---------|
| 2xx / 3xx | `Information` | `HTTP GET /api/clientes -> 200 \| 45ms \| Usuario: abc123` |
| 4xx | `Warning` | `HTTP POST /api/login -> 401 \| 8ms \| Usuario: anonimo \| IP: 192.168.1.1` |
| 5xx | `Error` | `HTTP POST /api/asientos -> 500 \| 123ms \| ...` |

### Excepciones — `ExceptionMiddleware`

Intercept toda excepción no controlada. Loggea con nivel diferenciado y devuelve JSON estructurado al cliente. **Nunca expone stack traces al cliente.**

| Tipo de excepción | Nivel de log | Datos capturados |
|-------------------|--------------|------------------|
| Excepciones de negocio (4xx) | `Warning` | Método, ruta, usuario, tipo de excepción, mensaje |
| `AccesoNoAutorizadoException` (403) | `Warning` | + dirección IP |
| Excepciones no mapeadas (500) | `Error` | + stack trace completo |

### Servicios — `ILogger<T>`

Todos los servicios de la capa Service reciben `ILogger<T>` por constructor y emiten logs estructurados para operaciones relevantes:

| Servicio | Eventos loggeados |
|----------|-------------------|
| `AuthService` | Login exitoso/fallido, registro, logout, login con Google |
| `AsientoContableService` | Creación/reversión de asientos, validaciones fallidas (balance, duplicados) |
| `ClienteService` | Alta, modificación, activación, desactivación |
| `CuentaContableService` | Alta, modificación, activación, desactivación de cuentas |
| `EjercicioContableService` | Apertura y cierre de ejercicios |
| `ComprobanteService` | Creación, modificación, anulación, generación de asiento |
| `UsuarioService` | Alta, edición, desactivación de usuarios |
| `RolService` | Creación/modificación de roles, asignación/remoción de permisos |
| `LibroMayorService` | `Warning` si la generación del reporte supera 2000 ms |
| `EstadoResultadosService` | `Warning` si la generación del reporte supera 2000 ms |
| `AuditoriaService` | `Error` si falla la persistencia de un registro de auditoría |
| `PermisoService` | Lecturas de permisos |

### UI — `ApiClient`

El cliente HTTP centralizado de la UI loggea automáticamente toda comunicación fallida con la API:

| Situación | Nivel | Ejemplo |
|-----------|-------|---------|
| Respuesta 4xx de la API | `Warning` | `API respondio 404 \| GET /api/clientes/999` |
| Respuesta 5xx de la API | `Error` | `API respondio 500 \| POST /api/asientos` |
| `HttpRequestException` / timeout | `Error` | `Error de comunicacion con la API \| POST /api/login` |

### Qué se puede ver en Azure Application Insights

- **Traces**: requests exitosos, operaciones de servicios, lecturas
- **Warnings**: validaciones de negocio fallidas, accesos denegados, requests 4xx, reportes lentos
- **Exceptions**: errores 500 con stack trace, fallos de persistencia de auditoría, errores de comunicación UI→API
- **Filtros disponibles** en `customDimensions`: `{UsuarioId}`, `{Ruta}`, `{Metodo}`, `{DuracionMs}`, `{TipoExcepcion}`

---

## Enumeraciones

| Campo | Entidad | Valores |
|-------|---------|---------|
| estado | Usuario | Activo, Inactivo |
| proveedorAuth | Usuario | Local, Google |
| estado | Cliente | Activo, Inactivo |
| tipoContribuyente | Cliente | CEDE, ResponsableIVA, Monotributo, LiteralE, NoAlcanzado, Exento |
| monedaBase | Cliente | UYU, USD |
| tipo | CuentaContable | Activo, Pasivo, Patrimonio, Ingreso, Egreso |
| naturaleza | CuentaContable | Deudora, Acreedora |
| estado | CuentaContable | Activa, Inactiva |
| estado | EjercicioContable | Abierto, Cerrado |
| estado | AsientoContable | Confirmado, Revertido |
| tipo | Comprobante | Compra, Venta |
| moneda | LineaAsiento / Comprobante | UYU, USD |
| fuenteOrigen | TipoDeCambio | BCU, Manual |
| estado | CentroDeCosto | Activo, Inactivo |

---

## Reglas de negocio críticas

- Un **asiento** debe estar balanceado (suma debe = suma haber). Nunca se modifica; se revierte y crea uno nuevo.
- Un **ejercicio cerrado** no admite operaciones y no puede reabrirse.
- El **contador_id** en Usuarios solo se completa para rol Auxiliar Contable. Administrador y Contador deben tener null.
- Un **cliente** pertenece al contador que lo creó. Los auxiliares heredan acceso a todos los clientes de su contador.
- Las **cuentas imputables** (es_imputable = true) admiten movimientos. Las agrupadoras (false) no.
- Los campos **datos_anteriores** y **datos_nuevos** de Auditoría almacenan JSON serializado.
- Las **contraseñas** se hashean con bcrypt (paquete BCrypt.Net-Next, work factor 12).
- El **JWT** dura 1 hora. No hay refresh token.

---

## Endpoints

### CRUD: `/api/{entidad}`
```
GET    /api/clientes
POST   /api/clientes
PUT    /api/clientes/{id}
GET    /api/asientos
POST   /api/asientos
...
```

### Reportes: `/api/reportes/{nombre}`
```
GET /api/reportes/libro-diario?clienteId=xxx&fechaDesde=...&fechaHasta=...
GET /api/reportes/libro-mayor?clienteId=xxx&cuentaId=...
GET /api/reportes/balance-general?clienteId=xxx&ejercicioId=...
GET /api/reportes/estado-resultados?clienteId=xxx&ejercicioId=...
GET /api/reportes/resumen-iva?clienteId=xxx&periodo=2025-03
```

Agregar `formato=pdf` o `formato=excel` para exportación.

### Paginación (todos los listados)
```
GET /api/asientos?clienteId=xxx&pagina=1&cantidadPorPagina=20
```

Respuesta: `{ datos: [], pagina, cantidadPorPagina, totalRegistros, totalPaginas }`
