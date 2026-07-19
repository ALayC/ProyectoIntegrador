# Guía de Onboarding (cómo correr el proyecto)

> Pasos mínimos para levantar la solución en un entorno local de desarrollo.

## 1. Prerrequisitos

- **.NET 8 SDK**
- **SQL Server** (local, LocalDB o Azure SQL)
- **Visual Studio 2022/2026** o **VS Code** + C# Dev Kit
- (Opcional) Cuenta de **Google Cloud** para OAuth y **Application Insights** para telemetría

## 2. Estructura de la solución

| Proyecto | Rol |
|---|---|
| `ProyectoIntegrador.UI` | Frontend (consume la API vía `ApiClient`). |
| `ProyectoIntegrador.API` | Web API (controllers, middlewares, auth). |
| `ProyectoIntegrador.Service` | Lógica de negocio, DTOs, excepciones. |
| `ProyectoIntegrador.Data` | Entidades, `AppDbContext`, repositorios, migraciones, seed. |
| `ProyectoIntegrador.Test` | Pruebas. |

## 3. Configuración (`appsettings.json` / User Secrets)

Configurar en la **API** (preferentemente con *User Secrets* en desarrollo, no en el repo):

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=...;Database=ProyectoIntegrador;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
	"Issuer": "...",
	"Audience": "...",
	"SecretKey": "clave-larga-y-secreta"
  },
  "Cors": { "OrigenesPermitidos": [ "https://localhost:xxxx" ] },
  "ApplicationInsights": { "ConnectionString": "" },
  "Email": { "...": "..." }
}
```

> Con `ApplicationInsights:ConnectionString` **vacío**, la telemetría no se activa (ideal para
> correr sin Azure). Swagger solo se habilita en desarrollo.

### Secretos (recomendado)
```powershell
cd ProyectoIntegrador.API
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "clave-larga-y-secreta"
```

## 4. Base de datos (migraciones EF Core)

```powershell
# desde la raíz de la solución
dotnet tool install --global dotnet-ef   # si no lo tenés

dotnet ef database update --project ProyectoIntegrador.Data --startup-project ProyectoIntegrador.API
```

Esto crea el esquema y carga el **seed data**: roles, permisos, usuario Administrador y plan de
cuentas *template*.

- **Usuario admin inicial:** el hash corresponde a la contraseña `Admin1234!` (cambiar en cuanto
  sea posible; ver `SeedData.SeedUsuarioAdmin`).

## 5. Ejecutar

Levantar **API** y **UI** (multi-startup en Visual Studio, o dos terminales):

```powershell
dotnet run --project ProyectoIntegrador.API
dotnet run --project ProyectoIntegrador.UI
```

- API: Swagger disponible en desarrollo (`/swagger`).
- UI: consume la API según la URL configurada en `ApiClient`.

## 6. Ejecutar pruebas

```powershell
dotnet test
```

## 7. Flujo de trabajo con migraciones

```powershell
# crear una migración tras cambiar entidades
dotnet ef migrations add NombreDescriptivo --project ProyectoIntegrador.Data --startup-project ProyectoIntegrador.API

# aplicarla
dotnet ef database update --project ProyectoIntegrador.Data --startup-project ProyectoIntegrador.API
```

## 8. Checklist rápido

- [ ] SQL Server accesible y connection string configurado.
- [ ] `Jwt:SecretKey` seteado (User Secrets).
- [ ] `dotnet ef database update` ejecutado correctamente.
- [ ] API responde en `/swagger`.
- [ ] Login con el usuario admin del seed.
