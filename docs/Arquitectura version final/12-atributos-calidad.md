# Requisitos No Funcionales (Atributos de Calidad)

> Los NFR describen *cómo* debe comportarse el sistema (rendimiento, seguridad, disponibilidad…),
> a diferencia de los requisitos funcionales que describen *qué* hace. Se incluyen metas medibles
> y el mecanismo con el que se abordan en el código/infraestructura.

## 1. Rendimiento

| Atributo | Meta | Mecanismo en el sistema |
|---|---|---|
| Tiempo de respuesta API | p95 < 2 s | Alerta configurada > 2 s durante 5 min. |
| Reportes contables | Consulta rápida sobre saldos | `SALDOS_CUENTA` pre-calculado (ADR-005). |
| Listados | Respuesta acotada | Paginación `OFFSET/FETCH` (máx. 100 por página). |
| Integración BCU | No bloquear la operación | Timeout 15 s + *fallback* manual (ADR-007). |

## 2. Seguridad

| Atributo | Meta | Mecanismo |
|---|---|---|
| Autenticación | Stateless, expiración corta | JWT 1 h + revocación por tabla (ADR-003). |
| Contraseñas | Almacenamiento seguro | bcrypt con salt (`BCrypt.Net-Next`). |
| Autorización | Mínimo privilegio | `PermisosActionFilter` + `[RequierePermiso]` (módulo/acción). |
| Aislamiento de datos | Sin fugas entre contadores | Filtrado por usuario autenticado (ADR-008). |
| Abuso / fuerza bruta | Contención | Rate limiting (global 200/min; login 10/15min; register 5/15min). |
| Superficie de exposición | Orígenes controlados | CORS `PermitirUI` con orígenes configurados. |
| Doble factor | Opcional / dispositivos confiables | `Codigo2FA` + `DispositivoConfiable`. |

## 3. Disponibilidad y resiliencia

| Atributo | Meta | Mecanismo |
|---|---|---|
| Continuidad ante caída del BCU | Operación no bloqueada | *Fallback* manual de cotización. |
| Recuperación de datos | RPO/RTO acotados | Azure SQL Backup + Point-in-Time Restore. |
| Degradación controlada | Errores consistentes | `ExceptionMiddleware` → respuesta JSON uniforme. |

## 4. Escalabilidad

| Atributo | Meta | Mecanismo |
|---|---|---|
| Carga de tráfico | Escalado horizontal | Azure App Service (UI y API separadas). |
| Crecimiento de datos | Consultas indexadas | Índices únicos y compuestos en `AppDbContext`. |
| Concurrencia contable | Integridad bajo carga | Transacción SERIALIZABLE en numeración (ADR-004). |

## 5. Mantenibilidad y testeabilidad

| Atributo | Meta | Mecanismo |
|---|---|---|
| Bajo acoplamiento | Capas intercambiables | Interfaces de repositorio y servicio + DI `Scoped`. |
| Testeabilidad | Lógica cubierta por tests | Servicios con repos mockeables; proyecto `ProyectoIntegrador.Test`. |
| Evolución del modelo | Cambios versionados | Migraciones EF Core en `ProyectoIntegrador.Data`. |

## 6. Observabilidad

| Atributo | Meta | Mecanismo |
|---|---|---|
| Trazabilidad de requests | Correlación por request | `RequestLoggingMiddleware` (método, ruta, status, duración, usuario, IP). |
| Diagnóstico en producción | Telemetría centralizada | Azure Application Insights (condicional al connection string). |
| Auditoría de negocio | Historial completo | Entidad `Auditoria` con `DatosAnteriores`/`DatosNuevos` (JSON). |

## 7. Alertas operativas (resumen)

| Alerta | Umbral | Acción |
|---|---|---|
| HTTP 5xx | > 5 % en 5 min | Notificación al equipo |
| Tiempo de respuesta | > 2 s durante 5 min | Revisión de consultas |
| Fallos al BCU | 3 consecutivos | Activar cotización manual |
| CPU App Service | > 80 % sostenido 10 min | Evaluar escalado |
