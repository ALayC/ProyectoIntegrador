using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoIntegrador.UI.Services;

namespace ProyectoIntegrador.UI.Controllers;

[Authorize]
public class AiController : Controller
{
    private readonly AiService _aiService;

    public AiController(AiService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest req)
    {
        var respuesta = await _aiService.ConsultarAsync(req.Pregunta);
        return Json(new { respuesta });
    }
}

public record ChatRequest(string Pregunta);
