using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UsuariosApp.Application.Dtos;
using UsuariosApp.Application.Helpers;
using UsuariosApp.Domain.Entities;
using UsuariosApp.Domain.Enums;
using UsuariosApp.InfraStructure.Context;
using UsuariosApp.InfraStructure.Repositories;

namespace UsuariosApp.InfraStructure.Tests.TestesRepository
{
    public class UsuarioRepositorytests
    {

        private readonly UsuarioDbContext _context;
        private readonly UsuarioRepository _usuarioRepository;
        private readonly Faker<UsuarioRequestDto> _faker;

        public UsuarioRepositorytests()
        {

            var options = new DbContextOptionsBuilder<UsuarioDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UsuarioDbContext(options);
            _usuarioRepository = new UsuarioRepository(_context);

            _faker = new Faker<UsuarioRequestDto>("pt_BR")
              .RuleFor(u => u.Nome, f => f.Person.FullName)
              .RuleFor(u => u.Email, f => f.Internet.Email())
              .RuleFor(u => u.Senha, f => f.Internet.Password())
              .RuleFor(u => u.Permissao, f => f.PickRandom<PermissaoEnum>());
        }

        [Fact]
        public async Task AdicionarAsync_DeveAdicionarUsuarioNoContexto()
        {
            var usuarioRequestDto = _faker.Generate();

            var senhaHash = $"HASHED_{usuarioRequestDto.Senha}";

            var usuario = new Usuario(
                usuarioRequestDto.Nome!,
                usuarioRequestDto.Email!,
                senhaHash,
                usuarioRequestDto.Permissao
            );

            var result = await _usuarioRepository.AdicionarAsync(usuario);

            await _context.SaveChangesAsync();

            var usuarioBanco = await _context.Usuarios.FindAsync(usuario.Id);

            usuarioBanco.Should().NotBeNull();
            usuarioBanco!.Nome.Should().Be(usuarioRequestDto.Nome);
            usuarioBanco.Email.Should().Be(usuarioRequestDto.Email);
            usuarioBanco.Permissao.Should().Be(usuarioRequestDto.Permissao);

            result.Should().Be(usuario);
        }

        [Fact]
        public async Task AtualizarAsync_DeveAtualizarUsuarioNoContexto()
        {
            var senhaHasher = new BCryptSenhaHasher();

            var usuarioRequestDto = _faker.Generate();

            var senhaHash = senhaHasher.HashPassword(usuarioRequestDto.Senha!);

            var usuario = new Usuario(
                 usuarioRequestDto.Nome!,
                 usuarioRequestDto.Email!,
                 senhaHash,
                 usuarioRequestDto.Permissao
             );

            await _usuarioRepository.AdicionarAsync(usuario);
            await _context.SaveChangesAsync();

            var novoEmail = "novo@email.com";

            var novaSenha = "NovaSenha@456";

            var novaSenhaHash = senhaHasher.HashPassword(novaSenha);

            usuario.AtualizarInformacoesUsuario(novoEmail, novaSenhaHash);

            var result = await _usuarioRepository.AtualizarAsync(usuario);

            await _context.SaveChangesAsync();

            var usuarioBanco = await _context.Usuarios.FindAsync(usuario.Id);

            usuarioBanco.Should().NotBeNull();
            usuarioBanco!.Email.Should().Be(novoEmail);
            usuarioBanco.SenhaHash.Should().Be(novaSenhaHash);

            result.Should().Be(usuario);
        }

        [Fact]
        public async Task ObterTodosUsuariosAsync_DeveRetornarUsuariosOrdenadosPorNome()
        {
            var dtoList = _faker.Generate(5);

            var usuarios = dtoList.Select(dto =>
                new Usuario(
                    dto.Nome!,
                    dto.Email!,
                    $"HASHED_{dto.Senha}",
                    dto.Permissao
                )
            )

            .OrderBy(u => Guid.NewGuid())
            .ToList();

            foreach (var usuario in usuarios)
            {
                await _usuarioRepository.AdicionarAsync(usuario);
            }
            await _context.SaveChangesAsync();

            var resultado = await _usuarioRepository.ObterTodosUsuariosAsync();

            resultado.Should().HaveCount(5);

            var nomesOrdenados = resultado.Select(u => u.Nome).OrderBy(n => n).ToList();
            resultado.Select(u => u.Nome).Should().BeEquivalentTo(nomesOrdenados, options => options.WithStrictOrdering());
        }

        [Fact]
        public async Task ObterTodosUsuariosAsync_DeveRetornarListaVazia_SeNaoExistirUsuarios()
        {
            var resultado = await _usuarioRepository.ObterTodosUsuariosAsync();

            resultado.Should().BeEmpty();
        }

        [Fact]
        public async Task ObterUsuarioPorIdAsync_DeveRetornarUsuario_QuandoExistir()
        {
            var usuarioRequestDto = _faker.Generate();

            var usuario = new Usuario(
                usuarioRequestDto.Nome!,
                usuarioRequestDto.Email!,
                $"HASHED_{usuarioRequestDto.Senha}",
                usuarioRequestDto.Permissao
            );

            await _usuarioRepository.AdicionarAsync(usuario);
            await _context.SaveChangesAsync();

            var resultado = await _usuarioRepository.ObterUsuarioPorIdAsync(usuario.Id);

            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(usuario.Id);
            resultado.Nome.Should().Be(usuario.Nome);
            resultado.Email.Should().Be(usuario.Email);
        }

        [Fact]
        public async Task ObterUsuarioPorIdAsync_DeveRetornarNull_QuandoNaoExistir()
        {
            var idInexistente = Guid.NewGuid();

            var resultado = await _usuarioRepository.ObterUsuarioPorIdAsync(idInexistente);

            resultado.Should().BeNull();
        }

        [Fact]
        public async Task RemoverAsync_DeveRemoverUsuario_QuandoExistir()
        {
            var usuarioRequestDto = _faker.Generate();

            var usuario = new Usuario(
                usuarioRequestDto.Nome!,
                usuarioRequestDto.Email!,
                $"HASHED_{usuarioRequestDto.Senha}",
                usuarioRequestDto.Permissao
            );

            await _usuarioRepository.AdicionarAsync(usuario);
            await _context.SaveChangesAsync();

            var resultado = await _usuarioRepository.RemoverAsync(usuario.Id);

            await _context.SaveChangesAsync();

            resultado.Should().BeTrue();

            var usuarioBanco = await _context.Usuarios.FindAsync(usuario.Id);
            usuarioBanco.Should().BeNull();
        }

        [Fact]
        public async Task RemoverAsync_DeveRetornarFalse_QuandoUsuarioNaoExistir()
        {
            var idInexistente = Guid.NewGuid();

            var resultado = await _usuarioRepository.RemoverAsync(idInexistente);

            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task UsuarioExisteAsync_DeveRetornarTrue_QuandoEmailExistir()
        {
            var usuarioRequestDto = _faker.Generate();

            var usuario = new Usuario(
                usuarioRequestDto.Nome!,
                usuarioRequestDto.Email!,
                $"HASHED_{usuarioRequestDto.Senha}",
                usuarioRequestDto.Permissao
            );

            await _usuarioRepository.AdicionarAsync(usuario);
            await _context.SaveChangesAsync();

            var existe = await _usuarioRepository.UsuarioExisteAsync(usuario.Email!);
            existe.Should().BeTrue();
        }

        [Fact]
        public async Task UsuarioExisteAsync_DeveRetornarFalse_QuandoEmailNaoExistir()
        {
            var emailNaoExistente = "naoexiste@email.com";

            var existe = await _usuarioRepository.UsuarioExisteAsync(emailNaoExistente);
            existe.Should().BeFalse();
        }


    }

}

