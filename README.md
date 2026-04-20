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

---

## 🚀 Guía de instalación y ejecución

### Prerequisitos

| Herramienta | Versión mínima | Descarga |
|-------------|---------------|----------|
| **Visual Studio 2022** | 17.8+ | [Descargar](https://visualstudio.microsoft.com/downloads/) |
| **.NET 8 SDK** | 8.0 | [Descargar](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **SQL Server** | LocalDB, Express o Developer | Incluido con VS o [descargar Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) |

> En Visual Studio Installer asegurarse de tener los workloads: **ASP.NET and web development** y **.NET desktop development**.

### Paso 1 — Clonar el repositorio

```bash
git clone https://github.com/ALayC/ProyectoIntegrador.git
cd ProyectoIntegrador
```

### Paso 2 — Abrir la solución

Abrir `ProyectoIntegrador.sln` con Visual Studio 2022.  
Los paquetes NuGet se restauran automáticamente. Si no, ejecutar:

```bash
dotnet restore
```

### Paso 3 — Configurar la conexión a SQL Server

Editar `ProyectoIntegrador.API\appsettings.json` según tu instancia:

**Windows Auth (por defecto):**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ProyectoIntegrador;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

**LocalDB (viene con Visual Studio):**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ProyectoIntegrador;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

**SQL Server con usuario/contraseña:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=ProyectoIntegrador;User Id=sa;Password=TuPassword;TrustServerCertificate=true;"
}
```

### Paso 4 — Crear la base de datos (ejecutar migraciones)

**Opción A — Package Manager Console** (en Visual Studio → Herramientas → NuGet → Package Manager Console):

```powershell
Update-Database -Project ProyectoIntegrador.Data -StartupProject ProyectoIntegrador.API
```

**Opción B — Terminal:**

```bash
dotnet ef database update --project ProyectoIntegrador.Data --startup-project ProyectoIntegrador.API
```

> Si no tienen `dotnet ef` instalado:
> ```bash
> dotnet tool install --global dotnet-ef
> ```

### Paso 5 — Configurar múltiples proyectos de inicio

1. Click derecho sobre la **Solución** → **Configure Startup Projects...**
2. Seleccionar **Multiple startup projects**
3. Poner en **Start**:
   - `ProyectoIntegrador.API`
   - `ProyectoIntegrador.UI`
4. Aceptar

### Paso 6 — Ejecutar

Presionar **F5** (debug) o **Ctrl+F5** (sin debug). Se abrirán:

| Proyecto | URL |
|----------|-----|
| **API** (Swagger) | `https://localhost:7225/swagger` |
| **UI** (Frontend) | `https://localhost:7160` |

### Paso 7 — Ejecutar los tests

**Opción A — Visual Studio:** Menú Test → Run All Tests (Ctrl+R, A)

**Opción B — Terminal:**

```bash
dotnet test
```

---

## ⚠️ Solución de problemas comunes

| Problema | Solución |
|----------|----------|
| Error de certificado HTTPS / `NET::ERR_CERT_INVALID` | Ejecutar `dotnet dev-certs https --trust` y reiniciar el navegador |
| `Cannot open database "ProyectoIntegrador"` | Verificar que SQL Server esté corriendo y el connection string sea correcto |
| `dotnet ef: command not found` | Ejecutar `dotnet tool install --global dotnet-ef` |
| La UI no conecta con la API | Verificar que ambos proyectos estén corriendo y que los puertos en `appsettings.json` coincidan con `launchSettings.json` |

---

## Convenciones de código

Ver [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) para las convenciones de nomenclatura, reglas por capa y estándares del proyecto.

## Estrategia de versionado

Se utiliza **GitHub Flow**:

1. La rama `main` siempre debe estar funcional.
2. Para cada tarea, crear rama con prefijo: `feature/`, `fix/`, `refactor/`, `docs/`.
3. Abrir PR hacia `main`. GitHub Actions compila y corre tests automáticamente.
4. Otro integrante revisa y aprueba. No se mergean PRs con tests fallidos.
5. Eliminar la rama después del merge.
