using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Models;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize]
public class CuentasContablesController : Controller
{
    private readonly ApiClient _apiClient;

    public CuentasContablesController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, Guid clienteId)
    {
        var response = await _apiClient.GetAsync<CuentaContableViewModel>($"api/cuentas-contables/{id}");

        if (response.EsNoAutorizado)
            return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso || response.Data is null)
        {
            TempData["Error"] = response.MensajeError ?? "Cuenta contable no encontrada.";
            return RedirectToAction("Index", "Clientes");
        }

        ViewData["ClienteId"] = clienteId;
        return View(response.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, Guid clienteId)
    {
        var response = await _apiClient.GetAsync<CuentaContableViewModel>($"api/cuentas-contables/{id}");

        if (response.EsNoAutorizado)
            return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso || response.Data is null)
        {
            TempData["Error"] = response.MensajeError ?? "Cuenta contable no encontrada.";
            return RedirectToAction("Index", "Clientes");
        }

        ViewData["ClienteId"] = clienteId;
        return View(response.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Guid clienteId, CuentaContableViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ClienteId"] = clienteId;
            return View(vm);
        }

        var response = await _apiClient.PutAsync<CuentaContableViewModel>($"api/cuentas-contables/{id}", new
        {
            codigo = vm.Codigo,
            nombre = vm.Nombre,
            tipo = vm.Tipo,
            naturaleza = vm.Naturaleza,
            esImputable = vm.EsImputable
        });

        if (response.EsNoAutorizado)
            return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "Error al actualizar la cuenta contable.");
            ViewData["ClienteId"] = clienteId;
            return View(vm);
        }

        TempData["Exito"] = "Cuenta contable actualizada correctamente.";
        return RedirectToAction(nameof(Details), new { id, clienteId });
    }

    [HttpGet]
    public async Task<IActionResult> CreateSubcuenta(Guid parentId, Guid clienteId)
    {
        var response = await _apiClient.GetAsync<CuentaContableViewModel>($"api/cuentas-contables/{parentId}");

        if (response.EsNoAutorizado)
            return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso || response.Data is null)
        {
            TempData["Error"] = response.MensajeError ?? "Cuenta contable no encontrada.";
            return RedirectToAction("Index", "Clientes");
        }

        var parent = response.Data;

        // Obtener el siguiente código sugerido para la subcuenta
        string codigoSugerido = string.Empty;
        var codigoResponse = await _apiClient.GetAsync<CodigoSugeridoDto>($"api/cuentas-contables/{parentId}/siguiente-codigo");
        if (codigoResponse.EsExitoso && codigoResponse.Data is not null)
            codigoSugerido = codigoResponse.Data.Codigo;

        var vm = new CuentaContableViewModel
        {
            PlanCuentasId = parent.PlanCuentasId,
            CuentaPadreId = parent.Id,
            Tipo = parent.Tipo,
            Naturaleza = parent.Naturaleza,
            EsSistema = false,
            Estado = "Activa",
            Codigo = codigoSugerido
        };

        var viewModel = new CuentaContableFormViewModel
        {
            Cuenta = vm,
            CuentaPadre = new CuentaContableResumenViewModel
            {
                Id = parent.Id,
                Codigo = parent.Codigo,
                Nombre = parent.Nombre
            }
        };

        ViewData["ClienteId"] = clienteId;

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSubcuenta(Guid clienteId, CuentaContableFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            if (form.Cuenta.CuentaPadreId is not null)
            {
                var parentLookupResponse = await _apiClient.GetAsync<CuentaContableViewModel>($"api/cuentas-contables/{form.Cuenta.CuentaPadreId}");

                if (parentLookupResponse.EsNoAutorizado)
                    return RedirectToAction("Login", "Auth");

                if (parentLookupResponse.EsExitoso && parentLookupResponse.Data is not null)
                {
                    form.CuentaPadre = new CuentaContableResumenViewModel
                    {
                        Id = parentLookupResponse.Data.Id,
                        Codigo = parentLookupResponse.Data.Codigo,
                        Nombre = parentLookupResponse.Data.Nombre
                    };
                }
            }

            ViewData["ClienteId"] = clienteId;
            return View(form);
        }

        if (form.Cuenta.CuentaPadreId is null)
        {
            TempData["Error"] = "Debe seleccionar una cuenta padre válida.";
            return RedirectToAction("Index", "Clientes");
        }

        var parentResponse = await _apiClient.GetAsync<CuentaContableViewModel>($"api/cuentas-contables/{form.Cuenta.CuentaPadreId}");

        if (parentResponse.EsNoAutorizado)
            return RedirectToAction("Login", "Auth");

        if (!parentResponse.EsExitoso || parentResponse.Data is null)
        {
            TempData["Error"] = parentResponse.MensajeError ?? "Cuenta padre no encontrada.";
            return RedirectToAction("Index", "Clientes");
        }

        var parent = parentResponse.Data;
        var response = await _apiClient.PostAsync<CuentaContableViewModel>($"api/cuentas-contables?planId={form.Cuenta.PlanCuentasId}", new
        {
            codigo = form.Cuenta.Codigo,
            nombre = form.Cuenta.Nombre,
            tipo = form.Cuenta.Tipo,
            naturaleza = form.Cuenta.Naturaleza,
            esImputable = form.Cuenta.EsImputable,
            cuentaPadreId = form.Cuenta.CuentaPadreId
        });

        if (response.EsNoAutorizado)
            return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            ModelState.AddModelError(string.Empty, response.MensajeError ?? "Error al crear la subcuenta.");
            form.CuentaPadre = new CuentaContableResumenViewModel
            {
                Id = parent.Id,
                Codigo = parent.Codigo,
                Nombre = parent.Nombre
            };
            ViewData["ClienteId"] = clienteId;
            return View(form);
        }

        TempData["Exito"] = "Subcuenta creada correctamente.";
        return RedirectToAction(nameof(Details), new { id = response.Data?.Id, planId = form.Cuenta.PlanCuentasId, clienteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activar(Guid id, Guid planId, Guid clienteId)
    {
        var response = await _apiClient.PostAsync<object>($"api/cuentas-contables/{id}/activar", new { });

        if (response.EsNoAutorizado)
            return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            TempData["Error"] = response.MensajeError ?? "Error al activar la cuenta contable.";
            return RedirectToAction(nameof(Details), new { id, planId, clienteId });
        }

        TempData["Exito"] = "Cuenta contable activada correctamente.";
        return RedirectToAction(nameof(Details), new { id, planId, clienteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(Guid id, Guid planId, Guid clienteId)
    {
        var response = await _apiClient.DeleteAsync($"api/cuentas-contables/{id}");

        if (response.EsNoAutorizado)
            return RedirectToAction("Login", "Auth");

        if (!response.EsExitoso)
        {
            TempData["Error"] = response.MensajeError ?? "Error al desactivar la cuenta contable.";
            return RedirectToAction(nameof(Details), new { id, planId, clienteId });
        }

        TempData["Exito"] = "Cuenta contable desactivada correctamente.";
        return RedirectToAction(nameof(Details), new { id, planId, clienteId });
    }
}
