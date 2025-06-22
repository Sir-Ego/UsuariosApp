using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UsuariosApp.API.MiddleWare;
using UsuariosApp.Application.Dtos;
using UsuariosApp.Domain.Enums;

namespace UsuariosApp.API.Tests.TestesMiddleware
{
    public class TratamentoExcecoesMiddlewareTests
    {
        private readonly HttpClient _client;
        private readonly Faker<UsuarioRequestDto> _faker;

        public TratamentoExcecoesMiddlewareTests()
        {

            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services => { });
                    builder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseMiddleware<TratamentoExcecoesMiddleware>();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapPost("/api/usuario", async context =>
                            {
                                var dto = await context.Request.ReadFromJsonAsync<JsonElement>();
                                var simularErro = dto.GetProperty("simularErro").GetString()?.ToLowerInvariant();

                                switch (simularErro)
                                {
                                    case "validation":
                                        var failures = new List<FluentValidation.Results.ValidationFailure>
                                        {
                                        new("Nome", "O nome é obrigatório."),
                                        new("Email", "O email está em formato inválido.")
                                        };

                                        throw new FluentValidation.ValidationException("Dados inválidos.", failures);

                                    case "nullargument":
                                        throw new ArgumentNullException("Campo", "Campo obrigatório ausente.");

                                    case "invalidargument":
                                        throw new ArgumentException("Argumento inválido.", "Campo");

                                    case "unauthorized":
                                        throw new UnauthorizedAccessException("Acesso não autorizado.");

                                    case "notimplemented":
                                        throw new NotImplementedException("Funcionalidade ainda não implementada.");

                                    case "notfound":
                                        throw new KeyNotFoundException("Usuario não encontrado");

                                    case "exception":
                                        throw new Exception("Erro fatal simulado.");

                                    default:
                                        await context.Response.WriteAsJsonAsync(new { sucesso = true });
                                        break;
                                }
                            });
                        });
                    });
                });

            _client = factory.CreateClient();
            _faker = new Faker<UsuarioRequestDto>("pt_BR")
              .RuleFor(u => u.Nome, f => f.Person.FullName)
              .RuleFor(u => u.Email, f => f.Internet.Email())
              .RuleFor(u => u.Senha, f => f.Internet.Password())
              .RuleFor(u => u.Permissao, f => f.PickRandom<PermissaoEnum>());
        }

        [Fact]
        public async Task Post_DeveRetornar400_QuandoValidationExceptionForLancada()
        {
            var usuarioRequestDto = _faker.Generate();
            var payload = new
            {
                usuarioRequestDto.Nome,
                usuarioRequestDto.Email,
                usuarioRequestDto.Senha,
                usuarioRequestDto.Permissao,
                simularErro = "validation"
            };


            var response = await _client.PostAsJsonAsync("/api/usuario", payload);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;

            json.GetProperty("mensagem").GetString().Should().Be("Erro de validação.");
        }

        [Fact]
        public async Task Post_DeveRetornar400_QuandoArgumentNullExceptionForLancada()
        {
            var usuarioRequestDto = _faker.Generate();
            var payload = new
            {
                usuarioRequestDto.Nome,
                usuarioRequestDto.Email,
                usuarioRequestDto.Senha,
                usuarioRequestDto.Permissao,
                simularErro = "nullargument"
            };

            var response = await _client.PostAsJsonAsync("/api/usuario", payload);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            json.GetProperty("mensagem").GetString().Should().Be("Campo obrigatório ausente.");
            json.GetProperty("erros").GetString().Should().Contain("Campo");
        }

        [Fact]
        public async Task Post_DeveRetornar400_QuandoArgumentExceptionForLancada()
        {
            var usuarioRequestDto = _faker.Generate();
            var payload = new
            {
                usuarioRequestDto.Nome,
                usuarioRequestDto.Email,
                usuarioRequestDto.Senha,
                usuarioRequestDto.Permissao,
                simularErro = "invalidargument"
            };

            var response = await _client.PostAsJsonAsync("/api/usuario", payload);
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            json.GetProperty("mensagem").GetString().Should().Be("Argumento inválido.");
            json.GetProperty("erros").GetString().Should().Contain("Campo");
        }

        [Fact]
        public async Task Post_DeveRetornar401_QuandoUnauthorizedAccessExceptionForLancada()
        {
            var usuarioRequestDto = _faker.Generate();
            var payload = new
            {
                usuarioRequestDto.Nome,
                usuarioRequestDto.Email,
                usuarioRequestDto.Senha,
                usuarioRequestDto.Permissao,
                simularErro = "unauthorized"
            };

            var response = await _client.PostAsJsonAsync("/api/usuario", payload);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;

            json.GetProperty("mensagem").GetString().Should().Be("Acesso não autorizado.");

        }

        [Fact]
        public async Task Post_DeveRetornar501_QuandoNotImplementedExceptionForLancada()
        {
            var usuarioRequestDto = _faker.Generate();
            var payload = new
            {
                usuarioRequestDto.Nome,
                usuarioRequestDto.Email,
                usuarioRequestDto.Senha,
                usuarioRequestDto.Permissao,
                simularErro = "notimplemented"
            };

            var response = await _client.PostAsJsonAsync("/api/usuario", payload);
            response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;

            json.GetProperty("mensagem").GetString().Should().Be("Funcionalidade ainda não implementada.");

        }

        [Fact]
        public async Task Post_DeveRetornar404_QuandoKeyNotFoundExceptionForLancada()
        {
            var usuarioRequestDto = _faker.Generate();
            var payload = new
            {
                usuarioRequestDto.Nome,
                usuarioRequestDto.Email,
                usuarioRequestDto.Senha,
                usuarioRequestDto.Permissao,
                simularErro = "notfound"
            };

            var response = await _client.PostAsJsonAsync("/api/usuario", payload);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;

            json.GetProperty("mensagem").GetString().Should().Be("Usuário não encontrado.");
            json.GetProperty("erros").GetString().Should().Contain("Usuario não encontrado");
        }

        [Fact]
        public async Task Post_DeveRetornar500_QuandoExceptionNaoTratadaForLancada()
        {
            var usuarioRequestDto = _faker.Generate();
            var payload = new
            {
                usuarioRequestDto.Nome,
                usuarioRequestDto.Email,
                usuarioRequestDto.Senha,
                usuarioRequestDto.Permissao,
                simularErro = "exception"
            };
            var response = await _client.PostAsJsonAsync("/api/usuario", payload);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            var erros = json.GetProperty("erros");

            erros.GetProperty("stackTrace").GetString().Should().NotBeNullOrEmpty();
            erros.GetProperty("tipo").GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RotaInexistente_DeveRetornar404()
        {
            var response = await _client.GetAsync("/api/inexistente");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}