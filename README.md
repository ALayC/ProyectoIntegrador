# ProyectoIntegrador - Sistema Contable Web

Sistema contable web desarrollado en .NET 8 para la gestión contable de clientes de un estudio contable en Uruguay.

## Estructura de la solución

```
ProyectoIntegrador/
├── ProyectoIntegrador.API/        → Capa de API (endpoints HTTP, JWT, validación)
├── ProyectoIntegrador.Data/       → Capa de Datos (entidades, repositorios, DbContext)
├── ProyectoIntegrador.Service/    → Capa de Servicios (lógica de negocio)
├── ProyectoIntegrador.Test/       → Tests unitarios (xUnit + Moq)
├── ProyectoIntegrador.UI/         → Capa de Presentación (Razor Views, HTML, CSS)
└── docs/                          → Documentación técnica del proyecto
    └── ARCHITECTURE.md            → Arquitectura, reglas y convenciones
```

## Referencias entre proyectos

```
UI  ──(HTTP)──▶  API  ──▶  Service  ──▶  Data
                                          │
Test  ──────────────────▶  Service  ──▶  Data
```

- **API** referencia a Service y Data
- **Service** referencia a Data
- **Test** referencia a Service y Data
- **UI** no referencia a ningún proyecto (consume la API por HTTP)
- **Data** no referencia a ningún proyecto

## Tecnologías

- .NET 8
- ASP.NET Core Web API + MVC (Razor Views)
- Entity Framework Core
- SQL Server
- xUnit + Moq (testing)
- Bootstrap (frontend)
- JWT + OAuth 2.0 Google (autenticación)

## Cómo ejecutar

Este proyecto requiere levantar dos procesos simultáneamente (API y UI):

1. En Visual Studio: clic derecho en la solución → Properties → Startup Project → Multiple Startup Projects → poner **ProyectoIntegrador.API** y **ProyectoIntegrador.UI** ambos en "Start".
2. Presionar F5 o Ctrl+F5.

## Convenciones de código

Ver [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) para las convenciones de nomenclatura, reglas por capa y estándares del proyecto.

## Estrategia de versionado

Se utiliza **GitHub Flow**:

1. La rama `main` siempre debe estar funcional.
2. Para cada tarea, crear rama con prefijo: `feature/`, `fix/`, `refactor/`, `docs/`.
3. Abrir PR hacia `main`. GitHub Actions compila y corre tests automáticamente.
4. Otro integrante revisa y aprueba. No se mergean PRs con tests fallidos.
5. Eliminar la rama después del merge.
