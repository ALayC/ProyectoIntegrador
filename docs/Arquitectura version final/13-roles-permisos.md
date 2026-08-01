# Roles y Permisos

> Reconstruido a partir de `ProyectoIntegrador.Data/Context/SeedData.cs`. El control de acceso se
> aplica con el filtro global `PermisosActionFilter` + `[RequierePermiso(modulo, accion)]`. Cada
> permiso es un par **Módulo + Acción**.

## 1. Roles predefinidos

| Rol | `EsPredefinido` | Descripción |
|---|---|---|
| **Administrador** | true | Gestión de usuarios del sistema. |
| **Contador** | true | Operativa contable completa sobre sus clientes. |
| **Auxiliar Contable** | true | Apoyo operativo; depende de un contador (`ContadorId`). |

> El usuario Administrador se crea como *seed* (`UsuarioAdminId`).

## 2. Matriz de permisos (según el seed real)

Convención: ✅ = permitido · — = no asignado.

| Módulo | Acción | Administrador | Contador | Auxiliar |
|---|---|:---:|:---:|:---:|
| Usuarios | Crear | ✅ | — | — |
| Usuarios | Consultar | ✅ | — | ✅ |
| Usuarios | Editar | ✅ | — | — |
| Usuarios | Desactivar | ✅ | — | — |
| Clientes | Crear | — | ✅ | — |
| Clientes | Consultar | — | ✅ | ✅ |
| Clientes | Editar | — | ✅ | — |
| Clientes | Desactivar | — | ✅ | — |
| Cuentas | Crear | — | ✅ | — |
| Cuentas | Consultar | — | ✅ | ✅ |
| Cuentas | Editar | — | ✅ | — |
| Cuentas | Desactivar | — | ✅ | — |
| Asientos | Crear | — | ✅ | ✅ |
| Asientos | Consultar | — | ✅ | ✅ |
| Asientos | Revertir | — | ✅ | — |
| Comprobantes | Crear | — | ✅ | ✅ |
| Comprobantes | Consultar | — | ✅ | ✅ |
| Comprobantes | Editar | — | ✅ | — |
| Comprobantes | Anular | — | ✅ | — |
| Importaciones | Crear | — | ✅ | — |
| Importaciones | Consultar | — | ✅ | ✅ |
| Reportes | Consultar | — | ✅ | ✅ |
| Reportes | Exportar | — | ✅ | — |
| Ejercicios | Crear | — | ✅ | — |
| Ejercicios | Consultar | — | ✅ | ✅ |
| Ejercicios | Editar | — | ✅ | — |
| Ejercicios | Desactivar | — | ✅ | — |
| CentrosCosto | Crear | — | ✅ | — |
| CentrosCosto | Consultar | — | ✅ | ✅ |
| CentrosCosto | Editar | — | ✅ | — |
| CentrosCosto | Desactivar | — | ✅ | — |

## 3. Resumen por rol

- **Administrador:** **solo** el módulo **Usuarios** (crear, consultar, editar, desactivar). No
  opera contablemente.
- **Contador:** **todos los permisos excepto** el módulo Usuarios. Es el rol operativo pleno.
- **Auxiliar Contable:** **consulta** en todos los módulos + **crear** Asientos y Comprobantes.
  No revierte, no anula, no exporta, no desactiva, no importa.

## 4. Reglas asociadas

- `Usuario.ContadorId` solo se completa para **Auxiliar**; es `null` para Administrador y Contador.
- El Auxiliar **hereda el acceso a los clientes** de su contador.
- Los permisos son **datos** (tabla `PERMISOS` + `ROLES_PERMISOS`): se pueden reasignar sin
  recompilar, respetando la convención Módulo/Acción.
