using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net.Http.Headers;

namespace Gateway.Filters;

// Attribut à poser sur les controllers/actions qui nécessitent un token valide
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireValidTokenAttribute : Attribute { }

public class TokenValidationFilter : IAsyncActionFilter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<TokenValidationFilter> _logger;

    public TokenValidationFilter(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<TokenValidationFilter> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Si la route n'a pas l'attribut [RequireValidToken], on laisse passer
        var hasAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<RequireValidTokenAttribute>().Any();

        if (!hasAttribute)
        {
            await next();
            return;
        }

        // Vérification du header Authorization
        var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(token))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Token manquant." });
            return;
        }

        // Validation du token via AuthService
        var isValid = await ValidateTokenAsync(token);
        if (!isValid)
        {
            _logger.LogWarning("Token refusé pour {Action}", context.ActionDescriptor.DisplayName);
            context.Result = new UnauthorizedObjectResult(new { error = "Token invalide ou expiré." });
            return;
        }

        await next();
    }

    private async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var baseUrl = _config["Services:AuthService"]
                ?? throw new InvalidOperationException("Services:AuthService non configuré.");

            if (token.StartsWith("Bearer "))
                token = token.Substring(7);

            var client = _httpClientFactory.CreateClient("AuthService");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Appel au endpoint /ping de l'AuthService pour valider le token
            var response = await client.GetAsync($"{baseUrl}/api/auth/ping");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la validation du token.");
            return false;
        }
    }
}