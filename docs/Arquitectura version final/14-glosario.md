# Glosario del Dominio

> Términos contables y del sistema para facilitar la comprensión a quien no conoce el negocio.

## Términos contables

| Término | Definición |
|---|---|
| **Asiento contable** | Registro de una operación en el libro diario, compuesto por líneas Debe/Haber que deben balancear (Debe = Haber). |
| **Línea de asiento** | Cada movimiento individual de un asiento sobre una cuenta, con importe en Debe o Haber, moneda y tipo de cambio. |
| **Glosa** | Descripción o concepto textual de un asiento. |
| **Debe / Haber** | Las dos columnas de la partida doble. Todo asiento debe cumplir que la suma del Debe iguale la del Haber. |
| **Plan de cuentas** | Estructura jerárquica de cuentas contables de un cliente. Existe un plan *template* del sistema. |
| **Cuenta contable** | Clasificación donde se imputan los movimientos (Activo, Pasivo, Patrimonio, Ingreso, Egreso). |
| **Cuenta imputable** | Cuenta que admite movimientos directos (`EsImputable=true`). Las no imputables son agrupadoras. |
| **Naturaleza** | Carácter de la cuenta: Deudora o Acreedora. |
| **Ejercicio contable** | Período (fecha inicio/fin) sobre el que se registran operaciones. Puede estar Abierto o Cerrado. |
| **Cierre de ejercicio** | Proceso que salda las cuentas de resultado, traslada el resultado a patrimonio y cierra el período. |
| **Asiento de reversión** | Asiento que corrige/anula otro (`AsientoOrigenId`), sin eliminar el original. |
| **Saldo de cuenta** | Acumulado (Debe/Haber/Saldo) por cuenta y período, pre-calculado para reportes. |
| **Comprobante** | Documento fiscal (Factura, Boleta, Nota de Débito/Crédito) asociado a un cliente. |
| **Centro de costo** | Segmentación para asignar costos/ingresos a una unidad de la organización. |
| **Libro Mayor** | Reporte con el detalle de movimientos y saldos por cuenta. |
| **Balance General** | Reporte del estado patrimonial (Activo, Pasivo, Patrimonio) a una fecha. |
| **Estado de Resultados** | Reporte de ingresos y egresos de un período. |
| **Liquidación de IVA** | Cálculo del IVA a pagar/crédito a partir de los comprobantes. |
| **Tipo de contribuyente** | Régimen tributario del cliente (CEDE, ResponsableIVA, Monotributo, LiteralE, NoAlcanzado, Exento). |

## Términos del sistema / técnicos

| Término | Definición |
|---|---|
| **Contador** | Usuario que gestiona sus propios clientes y su contabilidad. |
| **Auxiliar Contable** | Usuario de apoyo que depende de un contador y hereda acceso a sus clientes. |
| **Administrador** | Usuario que gestiona los usuarios del sistema. |
| **Template (plan)** | Plan de cuentas base del sistema (`EsTemplate=true`, `ClienteId=null`) que se copia al crear un cliente. |
| **Multi-tenancy lógico** | Aislamiento de datos por `ContadorId` dentro de una única base de datos. |
| **JWT** | Token de autenticación stateless con vigencia de 1 hora. |
| **2FA** | Autenticación en dos factores mediante código temporal. |
| **Dispositivo confiable** | Navegador/equipo recordado para no exigir 2FA en cada ingreso. |
| **BCU** | Banco Central del Uruguay; fuente de cotizaciones de moneda. |
| **Soft-delete** | Baja lógica preservando el registro (`DeletedAt` en Comprobante). |
| **Rate limiting** | Límite de solicitudes por ventana de tiempo para evitar abuso. |
| **Seed data** | Datos iniciales cargados en la base (roles, permisos, admin, plan template). |
| **DTO** | Objeto de transferencia de datos entre capas. |
| **ADR** | Registro de decisión de arquitectura. |
