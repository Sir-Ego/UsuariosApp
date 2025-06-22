using System.Net;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UsuariosApp.API.Controllers;
using UsuariosApp.Application.Dtos;
using UsuariosApp.Application.InterfaceService;
using UsuariosApp.Domain.Enums;

namespace UsuariosApp.API.Tests.TestesController
{
    public class UsuariosControllerTests
    {
        private readonly Mock<IUsuarioService> _mockService;
        private readonly UsuariosController _controller;
        private readonly Faker<UsuarioRequestDto> _faker;

        public UsuariosControllerTests()
        {
            _mockService = new Mock<IUsuarioService>();
            _controller = new UsuariosController(_mockService.Object);

            _faker = new Faker<UsuarioRequestDto>("pt_BR")
                .RuleFor(u => u.Nome, f => f.Person.FullName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Senha, f => f.Internet.Password())
                .RuleFor(u => u.Permissao, f => f.PickRandom<PermissaoEnum>());
        }

        [Fact]
        public async Task CreateUsuario_DeveRetornar201ComMensagem()
        {
            var dto = _faker.Generate();
            var usuarioCriado = new UsuarioResponseDto
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome,
                Email = dto.Email,
                Permissao = dto.Permissao.ToString(),
                DataCriacao = DateTime.UtcNow
            };

            _mockService.Setup(s => s.CreateUsuarioAsync(dto))
                        .ReturnsAsync(usuarioCriado);

            var result = await _controller.CreateUsuarioAsync(dto) as ObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be((int)HttpStatusCode.Created);
            result.Value.Should().BeEquivalentTo(new
            {
                message = "Usuario cadastrado com sucesso!",
                result = usuarioCriado
            });
        }

        [Fact]
        public async Task GetUsuarioById_DeveRetornarUsuario()
        {
            var id = Guid.NewGuid();
            var dto = _faker.Generate();
            var usuarioEsperado = new UsuarioResponseDto
            {
                Id = id,
                Nome = dto.Nome,
                Email = dto.Email,
                Permissao = dto.Permissao.ToString(),
                DataCriacao = DateTime.UtcNow
            };

            _mockService.Setup(s => s.GetUsuarioByIdAsync(id))
                        .ReturnsAsync(usuarioEsperado);

            var result = await _controller.GetUsuarioByIdAsync(id) as OkObjectResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            result.Value.Should().BeEquivalentTo(usuarioEsperado);
        }

        [Fact]
        public async Task GetAllUsuarios_DeveRetornarListaOuNoContent()
        {
            var id = Guid.NewGuid();
            var dto = _faker.Generate(5);
            var usuarioResponseDto = dto.Select(dto => new UsuarioResponseDto
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome,
                Email = dto.Email,
                Permissao = dto.Permissao.ToString(),
                DataCriacao = DateTime.UtcNow
            }).ToList();

            _mockService.Setup(s => s.GetAllUsuariosAsync()).ReturnsAsync(usuarioResponseDto);

            var result = await _controller.GetAllUsuariosAsync();

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(usuarioResponseDto);

            // Teste com lista vazia
            _mockService.Setup(s => s.GetAllUsuariosAsync()).ReturnsAsync(new List<UsuarioResponseDto>());

            var noContent = await _controller.GetAllUsuariosAsync();
            noContent.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task AtualizarConta_DeveRetornarOkComMensagem()
        {
            var id = Guid.NewGuid();
            var dto = _faker.Generate();
            var usuarioAtualizado = new UsuarioResponseDto
            {
                Id = id,
                Nome = dto.Nome,
                Email = dto.Email,
                Permissao = dto.Permissao.ToString(),
                DataCriacao = DateTime.UtcNow
            };


            _mockService.Setup(s => s.UpdateUsuarioAsync(dto, id))
                        .ReturnsAsync(usuarioAtualizado);

            var result = await _controller.AtualizarContaAsync(id, dto) as OkObjectResult;

            result.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(new
            {
                message = "Email e/ou senha atualizados com sucesso!",
                resultado = usuarioAtualizado
            });
        }

        [Fact]
        public async Task ExcluirUsuario_DeveRetornarOkComId()
        {
            var id = Guid.NewGuid();

            _mockService.Setup(s => s.DeleteUsuarioAsync(id)).Returns(Task.CompletedTask);

            var result = await _controller.ExcluirUsuarioAsync(id) as OkObjectResult;

            result.Should().NotBeNull();
            result.Value.Should().BeEquivalentTo(new
            {
                message = "Usuário excluído com sucesso!",
                idExcluido = id
            });
        }
    }
}
