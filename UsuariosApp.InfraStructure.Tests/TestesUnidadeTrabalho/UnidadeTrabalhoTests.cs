using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UsuariosApp.Application.Dtos;
using UsuariosApp.Domain.Entities;
using UsuariosApp.Domain.Enums;
using UsuariosApp.InfraStructure.Context;
using UsuariosApp.InfraStructure.UnidadesTrabalhos;

namespace UsuariosApp.InfraStructure.Tests.TestesUnidadeTrabalho
{
    public class UnidadeTrabalhoTests
    {
        private readonly DbContextOptions<UsuarioDbContext> _options;
        private readonly Faker<UsuarioRequestDto> _faker;

        public UnidadeTrabalhoTests()
        {
            _options = new DbContextOptionsBuilder<UsuarioDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _faker = new Faker<UsuarioRequestDto>("pt_BR")
                .RuleFor(u => u.Nome, f => f.Person.FullName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Senha, f => f.Internet.Password())
                .RuleFor(u => u.Permissao, f => f.PickRandom<PermissaoEnum>());
        }

        [Fact]
        public async Task CommitAsync_DeveSalvarAlteracoesNoBanco()
        {
            await using var context = new UsuarioDbContext(_options);
            var ut = new UnidadeTrabalho(context);

            var usuarioRequestDto = _faker.Generate();
            
            var usuario = new Usuario(
                usuarioRequestDto.Nome!,
                usuarioRequestDto.Email!,
                $"HASHED_{usuarioRequestDto.Senha}",
                usuarioRequestDto.Permissao
            );

            await context.Usuarios.AddAsync(usuario);
            await ut.CommitAsync();

            var total = await context.Usuarios.CountAsync();
            total.Should().Be(1);
        }

        [Fact]
        public async Task RollbackAsync_DeveReverterAdicao()
        {
            await using var context = new UsuarioDbContext(_options);
            
            var ut = new UnidadeTrabalho(context);

            var usuarioRequestDto = _faker.Generate();

            var usuario = new Usuario(
               usuarioRequestDto.Nome!,
               usuarioRequestDto.Email!,
               $"HASHED_{usuarioRequestDto.Senha}",
               usuarioRequestDto.Permissao
           );

            await context.Usuarios.AddAsync(usuario);
            await ut.RollbackAsync();

            var total = await context.Usuarios.CountAsync();
            total.Should().Be(0);
        }

        [Fact]
        public async Task RollbackAsync_DeveReverterModificacao()
        {
            await using var context = new UsuarioDbContext(_options);
            
            var ut = new UnidadeTrabalho(context);

            var usuarioRequestDto = _faker.Generate();

            var usuario = new Usuario(
                 usuarioRequestDto.Nome!,
                 usuarioRequestDto.Email!,
                 $"HASHED_{usuarioRequestDto.Senha}",
                 usuarioRequestDto.Permissao
             );

            await context.Usuarios.AddAsync(usuario);
            await context.SaveChangesAsync();

            // Altera o email e rola o rollback
            usuario.AtualizarInformacoesUsuario("rollback@email.com", usuario.SenhaHash!);
            await ut.RollbackAsync();

            var usuarioBanco = await context.Usuarios.FindAsync(usuario.Id);
            usuarioBanco!.Email.Should().Be(usuarioRequestDto.Email);
        }

        [Fact]
        public async Task RollbackAsync_DeveReverterRemocao()
        {
            await using var context = new UsuarioDbContext(_options);
            
            var ut = new UnidadeTrabalho(context);

            var usuarioRequestDto = _faker.Generate();

            var usuario = new Usuario(
                 usuarioRequestDto.Nome!,
                 usuarioRequestDto.Email!,
                 $"HASHED_{usuarioRequestDto.Senha}",
                 usuarioRequestDto.Permissao
             );

            await context.Usuarios.AddAsync(usuario);
            await context.SaveChangesAsync();

            context.Usuarios.Remove(usuario);
           
            await ut.RollbackAsync();

            var usuarioBanco = await context.Usuarios.FindAsync(usuario.Id);
            usuarioBanco.Should().NotBeNull();
        }

        [Fact]
        public void Dispose_DeveEncerrarContextoSemExcecao()
        {
            var context = new UsuarioDbContext(_options);
            var ut = new UnidadeTrabalho(context);

            Action acao = () => ut.Dispose();
            acao.Should().NotThrow();
        }
    }
}
