# Diagramas Mermaid (archivos individuales)

Cada archivo de esta carpeta contiene **solo la sintaxis Mermaid** de un diagrama (sin bloques ```` ```mermaid ````).
Están pensados para pegarse **directamente** en [mermaid.live](https://mermaid.live) sin el error `UnknownDiagramError`.

## Cómo usarlos

1. Abrí el archivo `.mmd` que quieras.
2. Copiá **todo** su contenido.
3. Pegalo en el editor de [mermaid.live](https://mermaid.live).
4. Exportá a PNG/SVG desde el botón *Actions → Export*.

> Importante: mermaid.live acepta **un solo diagrama a la vez**. No pegues el `.md` completo
> (`Arquitectura-Consolidado.md`), porque contiene varios diagramas + texto Markdown y por eso falla.
> Para ver todo el documento con los diagramas renderizados, usá la vista previa de Markdown de
> GitHub o de Visual Studio.

## Índice de diagramas

| Archivo | Tipo | Contenido |
|---|---|---|
| `01-arquitectura-capas.mmd` | flowchart TD | Arquitectura en capas (UI, API, Servicios, Datos) |
| `02-mer.mmd` | erDiagram | Modelo Entidad–Relación (MER) |
| `03-clases.mmd` | classDiagram | Diagrama de clases del dominio |
| `04-secuencia-login-google.mmd` | sequenceDiagram | Login con Google (OAuth 2.0) |
| `05-secuencia-importacion-excel.mmd` | sequenceDiagram | Importación desde Excel (dos fases) |
| `06-secuencia-registro-asiento.mmd` | sequenceDiagram | Registro de asiento contable |
| `07-flujo-cierre-ejercicio.mmd` | flowchart TD | Cierre de ejercicio contable |
| `08-despliegue-azure.mmd` | flowchart LR | Infraestructura y despliegue en Azure |
| `09-seguridad-flujo.mmd` | flowchart TD | Seguridad y autenticación (JWT, 2FA, permisos, rate limiting) |
| `10-errores-logging.mmd` | flowchart TD | Manejo de errores, mapeo excepción→HTTP y logging |
| `11-api-estandares.mmd` | flowchart LR | Estándares de la API (rutas, paginación, controllers) |
| `12-enums-reglas.mmd` | flowchart TD | Enumeraciones y reglas de negocio |
