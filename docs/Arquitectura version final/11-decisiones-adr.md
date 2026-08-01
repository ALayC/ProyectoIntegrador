# Decisiones de Arquitectura (ADR)

> **ADR = Architecture Decision Record.** Cada registro documenta una decisión relevante:
> su **contexto**, la **decisión** tomada, las **alternativas** consideradas y sus
> **consecuencias**. Sirven para responder *"¿por qué está hecho así?"* — algo que los
> diagramas no muestran.

---

## ADR-001 — Arquitectura en capas (no microservicios / no DDD)

- **Estado:** Aceptada.
- **Contexto:** Sistema contable de tamaño medio, desarrollado por un equipo pequeño, con
  necesidad de mantenibilidad y de repartir trabajo entre integrantes.
- **Decisión:** Adoptar arquitectura en **capas** (UI → API → Servicios → Datos) con patrón
  **Repository + Service Layer** e interfaces para desacoplar.
- **Alternativas consideradas:**
  - *Microservicios:* descartado por complejidad operativa (despliegue, red, observabilidad)
	desproporcionada para el tamaño del proyecto.
  - *Domain-Driven Design táctico (agregados ricos):* descartado por sobrecoste de modelado
	frente al beneficio; el dominio contable es estable y bien conocido.
- **Consecuencias:**
  - (+) Separación clara de responsabilidades, testeabilidad, trabajo paralelo.
  - (+) Curva de aprendizaje baja.
  - (−) Un único despliegue monolítico por proceso; el escalado es por instancia, no por módulo.

---

## ADR-002 — Modelo anémico + lógica en la capa de servicios

- **Estado:** Aceptada.
- **Contexto:** Las entidades EF Core se usan tanto para persistencia como para transporte.
- **Decisión:** Mantener las entidades como **POCOs** (solo propiedades + navegación). Toda la
  lógica de negocio (validar balance, revertir asiento, calcular IVA, recalcular saldos) vive en
  los **servicios**.
- **Alternativas:** Modelo de dominio rico (métodos de negocio en las entidades) — descartado
  para evitar acoplar reglas al mapeo ORM y facilitar el mockeo de repositorios.
- **Consecuencias:**
  - (+) Servicios fáciles de testear con repositorios mockeados vía interfaz.
  - (−) Riesgo de *anemia*: hay que disciplinar que la lógica no se filtre a controllers.

---

## ADR-003 — JWT de 1 hora sin refresh token + revocación por tabla

- **Estado:** Aceptada.
- **Contexto:** Autenticación stateless entre UI y API desplegadas como orígenes distintos.
- **Decisión:** Emitir **JWT de 1 hora**, sin refresh token. El logout registra el token en
  `TOKENS_REVOCADOS`; la revocación se valida en `OnTokenValidated`. `ClockSkew = Zero`.
- **Alternativas:**
  - *Refresh tokens:* mayor complejidad (rotación, almacenamiento seguro) no justificada para el
	caso de uso actual.
  - *Solo JWT sin revocación:* imposibilita invalidar sesiones tras logout.
- **Consecuencias:**
  - (+) Simplicidad; capacidad de invalidar sesiones.
  - (−) Cada request valida revocación (consulta a repositorio); expiración corta obliga a
	re-login más seguido.

---

## ADR-004 — Numeración de asientos con transacción SERIALIZABLE

- **Estado:** Aceptada.
- **Contexto:** El número de asiento debe ser **único y correlativo** por cliente + ejercicio,
  bajo posible concurrencia.
- **Decisión:** Asignar el número dentro de una transacción con nivel de aislamiento
  **SERIALIZABLE**, bloqueando lecturas concurrentes hasta finalizar.
- **Alternativas:**
  - *Columna IDENTITY / secuencia:* no permite reiniciar por cliente/ejercicio ni control fino.
  - *Optimistic concurrency + reintentos:* más código y reintentos ante colisión.
- **Consecuencias:**
  - (+) Garantía fuerte de unicidad y correlatividad.
  - (−) Menor concurrencia para el mismo cliente/ejercicio durante la transacción.

---

## ADR-005 — `SaldoCuenta` pre-calculado (materializado)

- **Estado:** Aceptada.
- **Contexto:** Los reportes (Libro Mayor, Balance, Estado de Resultados) requieren saldos por
  cuenta y período; recalcularlos sumando todas las líneas sería costoso.
- **Decisión:** Mantener `SALDOS_CUENTA` **pre-calculado** por cuenta/período/moneda, actualizado
  transaccionalmente al confirmar o revertir asientos.
- **Alternativas:** Calcular al vuelo con agregaciones SQL — descartado por rendimiento en
  reportes frecuentes.
- **Consecuencias:**
  - (+) Reportes rápidos.
  - (−) Complejidad extra: toda operación que afecte líneas debe mantener los saldos consistentes.

---

## ADR-006 — Persistencia de enums como texto

- **Estado:** Aceptada.
- **Contexto:** Varios campos representan conjuntos cerrados de valores (estados, tipos).
- **Decisión:** `Comprobante.Tipo` y `Comprobante.Estado` son **enums de C#** persistidos como
  **string** (`HasConversion<string>`); el resto de los "enumerados" se guardan como texto libre
  con valores convenidos.
- **Alternativas:** Persistir enums como `int` — descartado por legibilidad directa en base de
  datos y estabilidad ante reordenamientos del enum.
- **Consecuencias:**
  - (+) Datos autoexplicativos en la base; sin acoplar el orden del enum al valor almacenado.
  - (−) Ocupa algo más de espacio; requiere validar strings para los campos no tipados como enum.

---

## ADR-007 — Integración con el BCU con *fallback* manual

- **Estado:** Aceptada.
- **Contexto:** Se necesitan cotizaciones para la conversión multimoneda; el servicio del BCU
  puede no estar disponible.
- **Decisión:** `TipoDeCambioService` consume el BCU (HttpClient "BCU", timeout 15s) y cachea en
  `TIPOS_CAMBIO`. Si el BCU no responde, se admite **ingreso manual** (`FuenteOrigen=Manual`) sin
  bloquear la operación.
- **Alternativas:** Depender solo del BCU — descartado por riesgo de indisponibilidad.
- **Consecuencias:**
  - (+) Continuidad operativa ante caídas del BCU; historial trazable de la fuente.
  - (−) Posible cotización manual menos precisa; requiere política de conciliación posterior.

---

## ADR-008 — Aislamiento de datos por contador (multi-tenancy lógico)

- **Estado:** Aceptada.
- **Contexto:** Cada contador gestiona sus propios clientes; no debe ver datos de otros.
- **Decisión:** Aislamiento **lógico** mediante `Cliente.ContadorId` y filtrado por usuario
  autenticado en las consultas. Los auxiliares heredan el acceso de su contador (`ContadorId`).
- **Alternativas:** Base de datos por tenant — descartado por sobrecoste operativo para la escala
  actual.
- **Consecuencias:**
  - (+) Simplicidad y una sola base.
  - (−) El aislamiento depende de aplicar el filtro **en todas** las consultas: es una invariante
	crítica de seguridad que debe testearse.

> **Plantilla para nuevas ADR**
> `## ADR-NNN — Título` · **Estado** · **Contexto** · **Decisión** · **Alternativas** · **Consecuencias**
