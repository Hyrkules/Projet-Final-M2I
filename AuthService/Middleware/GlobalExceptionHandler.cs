using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace AuthService.Middleware
{
    // Elle va s'insérer dans la pipeline de la requete http
    public class GlobalExceptionHandler
    {
        // "RequestDelegate" représente le middleware suivant de la chaine 
        // C'est comme le relais : je fais mon travail puis je passe le témoin au suivant
        private readonly RequestDelegate _next;

        public GlobalExceptionHandler(RequestDelegate next)
        {
            _next = next;
        }


        // Methode "InvokeAsync" qui est appelée automatiquement à chaque requête HTTP
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Je laisse passer la requete vers la suite (le controller)
                // "Allez y, faites votre travail !"
                await _next(context);
            }
            catch (Exception ex)
            {
                // si quelque chose a planté n'importe où après moi, je tombe ici
                // Je declanche la methode de secours
                // Quand le service fait un throw new ... :
                // - L'exception remonte la pile d'appels :
                //      Service.GetById() => Controller.GetById() =>                Middleware.InvokeAsync()
                // - Comme il n y a pas de try catch dans le service ni dans le controller ni ailleurs, l'exception traverse toute les couches sans etre stoppé
                // - Elle arrive ici dans le catch du middleware(le seul try catch du projet)
                // - le middleware analyse le tye de l'exception , fabrique une réponce http adapté (404, 400, 500...)
                // - LA réponse JSON est envoyé au client . FIN !

                await HandleExceptionAsync(context, ex);
            }
        }

        // Elle fabrique la réponse json en cas d'erreur
        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            //on dit au client : "je te réponds du json"
            context.Response.ContentType = "application/json";

            //Par défault, on part du principe que c'est une erreur grave (500)
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // On utilise l''objet standart "ProblemDetails" recommandé par microsoft
            var response = new ProblemDetails
            {
                Title = "Une erreur est survenue sur le server",
                Detail = exception.Message
            };

            // On analyse le type de l'exception pour donner le bon code http
            switch (exception)
            {
                // CAS : Je ne trouve pas : 404 not found
                //case NotFoundException:
                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Title = "Ressource est introuvable";
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Title = "Accès refusé.";
                    break;

                // CAS Tu m'as donné n'importe quoi 400 Badrequest
                case ArgumentOutOfRangeException:
                case ArgumentNullException:
                case ArgumentException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                //CAs Tout le reste  500 internal server error
                // Erreur grave, bug, probleme technique imprévu
                default:
                    break;
            }

            // on transforme l"objet C# en texte json
            var json = JsonSerializer.Serialize(response);

            // on l'écrit dans le réponse http
            return context.Response.WriteAsync(json);
        }
    }
}
