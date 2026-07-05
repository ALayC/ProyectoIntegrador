using System.Net.Http.Json;
using System.Text.Json;

namespace ProyectoIntegrador.UI.Services;

public class AiService
{
    private const string SystemPrompt =
        "Sos un asistente contable experto en normativa uruguaya (DGI, BPS, IRAE, IRPF). " +
        "Siempre tenes que responder para la normativa de uruguay " +
        "Respondé siempre en español rioplatense, de forma concisa y práctica. " +
        "Si no sabés algo con certeza, decilo.";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<string> ConsultarAsync(string pregunta)
    {
        try
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = $"{SystemPrompt}\n\n{pregunta}" }
                        }
                    }
                }
            };

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(url, body);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);
                await Task.Delay(retryAfter);
                response = await client.PostAsJsonAsync(url, body);
                if (!response.IsSuccessStatusCode)
                    return "Límite de consultas alcanzado. Esperá unos segundos y volvé a intentar.";
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "No se pudo obtener una respuesta.";
        }
        catch
        {
            return "Ocurrió un error al conectar con el asistente. Intentá de nuevo más tarde.";
        }
    }
}
