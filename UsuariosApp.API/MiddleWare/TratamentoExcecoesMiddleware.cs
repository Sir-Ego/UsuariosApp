using System.Net;
using System.Text.Json;
using FluentValidation;


namespace UsuariosApp.API.MiddleWare
{

    public class TratamentoExcecoesMiddleware
    {
        private readonly RequestDelegate _next;

        public TratamentoExcecoesMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                await TratarExcecaoAsync(context, HttpStatusCode.BadRequest, "Erro de validação.", ex.Errors
                    .Select(e => new { campo = e.PropertyName, erro = e.ErrorMessage }));
            }
            catch (ArgumentNullException ex)
            {
                await TratarExcecaoAsync(context, HttpStatusCode.BadRequest, "Campo obrigatório ausente.", ex.Message);
            }
            catch (ArgumentException ex)
            {
                await TratarExcecaoAsync(context, HttpStatusCode.BadRequest, "Argumento inválido.", ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                await TratarExcecaoAsync(context, HttpStatusCode.Unauthorized, "Acesso não autorizado.", ex.Message);
            }
            catch (NotImplementedException ex)
            {
                await TratarExcecaoAsync(context, HttpStatusCode.NotImplemented, "Funcionalidade ainda não implementada.", ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                await TratarExcecaoAsync(context, HttpStatusCode.NotFound, "Usuário não encontrado.", ex.Message);
            }
            catch (Exception ex)
            {
                var detalhesErro = new
                {
                    ex.Message,
                    ex.StackTrace,
                    Tipo = ex.GetType().FullName
                };

                await TratarExcecaoAsync(context, HttpStatusCode.InternalServerError, "Erro interno na aplicação.", detalhesErro);
            }

        }

        private async Task TratarExcecaoAsync(HttpContext context, HttpStatusCode status, string mensagem, object erros)
        {
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";

            var resposta = new
            {
                status = (int)status,
                sucesso = false,
                mensagem,
                erros,
                data = DateTime.UtcNow,
                rota = context.Request.Path,
                traceId = context.TraceIdentifier
            };

            var json = JsonSerializer.Serialize(resposta, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            await context.Response.WriteAsync(json);
        }
    }
}
