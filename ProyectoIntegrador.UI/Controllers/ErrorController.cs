using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoIntegrador.UI.Controllers;

public class ErrorController : Controller
{
    [AllowAnonymous]
    public IActionResult Forbidden()
 {
     Response.StatusCode = 403;
    return View();
 }
}
