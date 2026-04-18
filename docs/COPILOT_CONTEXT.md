# Contexto del proyecto para asistentes de IA

## Instrucciones para el asistente

Sos un desarrollador senior de .NET 8 trabajando en un sistema contable web para Uruguay. Antes de generar cualquier código, leé el archivo `docs/ARCHITECTURE.md` que tiene las reglas, convenciones y restricciones del proyecto. Respetá SIEMPRE la separación en capas y las convenciones de nomenclatura definidas ahí.

## Sobre el proyecto

Sistema contable web para un estudio contable en Uruguay. Permite al contador y sus auxiliares gestionar la contabilidad de múltiples clientes: registrar asientos, importar comprobantes desde Excel, generar reportes (libro diario, libro mayor, balance general, estado de resultados, resumen de IVA) y manejar múltiples monedas con tipo de cambio del BCU.

## Stack tecnológico

- .NET 8
- ASP.NET Core Web API (capa API)
- ASP.NET Core MVC con Razor Views (capa UI)
- Entity Framework Core (ORM)
- SQL Server (base de datos)
- xUnit + Moq (testing)
- JWT + OAuth 2.0 Google (autenticación)
- bcrypt vía BCrypt.Net-Next (hashing de contraseñas)
- Bootstrap (frontend)

## Estructura de la solución (5 proyectos)

```
ProyectoIntegrador.API      → Controllers, JWT, middleware de errores, rate limiting
ProyectoIntegrador.Service  → Lógica de negocio, interfaces de servicios, DTOs, excepciones
ProyectoIntegrador.Data     → Entidades, repositorios, DbContext, migraciones
ProyectoIntegrador.Test     → Tests unitarios de servicios (xUnit + Moq)
ProyectoIntegrador.UI       → Razor Views, consume la API por HTTP
```

Referencias: API → Service, Data | Service → Data | Test → Service, Data | UI → ninguno (HTTP) | Data → ninguno

## Entidades del dominio (modelo de datos completo)

### Seguridad y acceso

**Usuario**: Id (Guid), Email (string), PasswordHash (string?), NombreCompleto (string), ProveedorAuth (enum: Local/Google), Estado (enum: Activo/Inactivo), RolId (Guid FK→Rol), ContadorId (Guid? FK→Usuario, solo para auxiliares), CreatedAt (DateTime)

**Rol**: Id (Guid), Nombre (string), EsPredefinido (bool). Roles predefinidos: Administrador, Contador, Auxiliar Contable.

**Permiso**: Id (Guid), Nombre (string), Modulo (string), Accion (string)

**RolPermiso**: RolId (Guid PK FK), PermisoId (Guid PK FK). Tabla intermedia muchos-a-muchos.

**TokenRevocado**: Id (Guid), UsuarioId (Guid FK), Token (string), ExpiraEn (DateTime)

### Clientes y estructura contable

**Cliente**: Id (Guid), ContadorId (Guid FK→Usuario), Rut (string, único), RazonSocial (string), NombreFantasia (string?), Email (string), Telefono (string), TipoContribuyente (enum: CEDE/ResponsableIVA/Monotributo/LiteralE/NoAlcanzado/Exento), MonedaBase (enum: UYU/USD), Estado (enum: Activo/Inactivo)

**PlanDeCuentas**: Id (Guid), ClienteId (Guid FK, relación 1:1 con Cliente)

**CuentaContable**: Id (Guid), PlanCuentasId (Guid FK), CuentaPadreId (Guid? FK autorreferencia), Codigo (string, único dentro del plan), Nombre (string), Tipo (enum: Activo/Pasivo/Patrimonio/Ingreso/Egreso), Naturaleza (enum: Deudora/Acreedora), EsImputable (bool), Estado (enum: Activa/Inactiva)

**EjercicioContable**: Id (Guid), ClienteId (Guid FK), FechaInicio (DateTime), FechaFin (DateTime), Estado (enum: Abierto/Cerrado). Soporta calendario móvil. No se pueden solapar ejercicios del mismo cliente.

### Núcleo transaccional

**AsientoContable**: Id (Guid), ClienteId (Guid FK), UsuarioId (Guid FK), EjercicioId (Guid FK), AsientoOrigenId (Guid? FK autorreferencia para reversiones), Numero (int, secuencial por cliente+ejercicio), Fecha (DateTime), Glosa (string), Estado (enum: Confirmado/Revertido)

**LineaAsiento**: Id (Guid), AsientoId (Guid FK), CuentaContableId (Guid FK), CentroCostoId (Guid? FK), Debe (decimal 18,2), Haber (decimal 18,2), Moneda (enum: UYU/USD), TipoCambio (decimal 10,4), ImporteMonedaBase (decimal 18,2)

**Comprobante**: Id (Guid), ClienteId (Guid FK), ImportacionId (Guid? FK), AsientoId (Guid? FK), Tipo (enum: Compra/Venta), Numero (string), FechaEmision (DateTime), FechaContable (DateTime), RutContraparte (string), Moneda (enum: UYU/USD), ImporteNeto (decimal 18,2), TasaIva (decimal 5,2), ImporteIva (decimal 18,2)

**Importacion**: Id (Guid), UsuarioId (Guid FK), ClienteId (Guid FK), NombreArchivo (string), FechaImportacion (DateTime), RegistrosImportados (int), RegistrosRechazados (int)

**SaldoCuenta**: Id (Guid), ClienteId (Guid FK), CuentaContableId (Guid FK), EjercicioId (Guid FK), Periodo (string "AAAA-MM"), DebeAcumulado (decimal 18,2), HaberAcumulado (decimal 18,2), Saldo (decimal 18,2)

### Soporte transversal

**CentroDeCosto**: Id (Guid), ClienteId (Guid FK), Codigo (string), Nombre (string), Estado (enum: Activo/Inactivo)

**TipoDeCambio**: Id (Guid), Moneda (string), Fecha (DateTime), Valor (decimal 10,4), FuenteOrigen (enum: BCU/Manual)

**Auditoria**: Id (Guid), UsuarioId (Guid FK), Entidad (string), Accion (string), FechaHora (DateTime), DatosAnteriores (string? JSON), DatosNuevos (string? JSON)

## Reglas de negocio críticas

1. Un asiento DEBE estar balanceado (suma debe = suma haber). Nunca se modifica un asiento confirmado; se revierte y crea uno nuevo.
2. Un ejercicio cerrado NO admite operaciones y NO puede reabrirse.
3. El cierre de ejercicio genera: diferencias de cambio → asiento de cierre → arrastre de saldos al nuevo ejercicio.
4. El campo ContadorId en Usuario solo se completa para rol Auxiliar Contable. Para Administrador y Contador debe ser null.
5. Un cliente pertenece al contador (ContadorId en Clientes). Los auxiliares del contador heredan acceso a todos sus clientes.
6. Solo se pueden imputar movimientos a cuentas con EsImputable = true.
7. Las contraseñas se hashean con bcrypt (work factor 12).
8. El JWT dura 1 hora, sin refresh token.
9. Los campos DatosAnteriores/DatosNuevos de Auditoría son JSON serializado.
10. La numeración de asientos usa transacción SERIALIZABLE para evitar duplicados por concurrencia.

## Excepciones de dominio

| Excepción | HTTP | Cuándo |
|-----------|------|--------|
| AsientoNoBalanceadoException | 400 | Debe ≠ Haber |
| EjercicioCerradoException | 400 | Operar en ejercicio cerrado |
| CuentaNoImputableException | 400 | Movimiento contra cuenta agrupadora |
| EntidadNoEncontradaException | 404 | Recurso inexistente |
| AccesoNoAutorizadoException | 403 | Sin permisos |
| DuplicadoException | 409 | RUT o email ya existente |
| EjercicioSolapadoException | 400 | Ejercicio se solapa con otro |
| ImportacionInvalidaException | 400 | Excel con estructura incorrecta |

## Endpoints

CRUD: `/api/{entidad}` (GET, POST, PUT)
Reportes: `/api/reportes/{nombre}` (libro-diario, libro-mayor, balance-general, estado-resultados, resumen-iva)
Paginación en todos los listados: `?pagina=1&cantidadPorPagina=20`
Exportación: agregar `?formato=pdf` o `?formato=excel`

## Convenciones de código

- Clases/Métodos/Propiedades: PascalCase
- Variables/Parámetros: camelCase
- Interfaces: I + PascalCase
- Tests: MetodoQueTesteas_Escenario_ResultadoEsperado
- Ramas: feature/, fix/, refactor/, docs/
- Commits: descriptivos en español
