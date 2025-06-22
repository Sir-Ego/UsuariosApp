using Bogus;
using FluentAssertions;
using System;
using UsuariosApp.Domain.Entities;
using UsuariosApp.Domain.Enums;
using Xunit;

namespace UsuariosApp.Domain.Tests.Entities
{
    public class UsuarioTests
    {
        private readonly Faker _faker;
        private readonly Faker<Usuario> _fakerUsuario;

        public UsuarioTests()
        {
            _faker = new Faker("pt_BR");

            _fakerUsuario = new Faker<Usuario>("pt_BR")
               .CustomInstantiator(f => new Usuario(
                   f.Person.FullName,
                   f.Internet.Email(),
                   $"HASHED_{f.Internet.Password()}",
                   f.PickRandom<PermissaoEnum>()
               ));
        }

        [Fact]
        public void CriarUsuario_Valido_DeveFuncionar()
        {
            // Gerar valores separados para comparar
            var nome = _faker.Person.FullName;
            var email = _faker.Internet.Email();
            var senhaHash = $"HASHED_{_faker.Internet.Password()}";
            var dataCriacao = DateTime.UtcNow;
            var permissao = _faker.PickRandom<PermissaoEnum>();

            var usuario = new Usuario(nome, email, senhaHash, permissao);

            usuario.Nome.Should().Be(nome);
            usuario.Email.Should().Be(email);
            usuario.SenhaHash.Should().Be(senhaHash);
            usuario.DataCriacao.Should().BeCloseTo(dataCriacao, TimeSpan.FromSeconds(1));
            usuario.Permissao.Should().Be(permissao);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Jo")]
        [InlineData("123")]
        [InlineData("!@#$")]
        public void CriarUsuario_ComNomeInvalido_DeveLancarArgumentException(string nomeInvalido)
        {
            var email = _faker.Internet.Email();
            var senhaHash = $"HASHED_{_faker.Internet.Password()}";
            var permissao = _faker.PickRandom<PermissaoEnum>();

            Action acao = () => new Usuario(nomeInvalido, email, senhaHash, permissao);

            acao.Should().Throw<ArgumentException>().WithMessage("*nome*");
        }

        [Theory]
        [InlineData("")]
        [InlineData("sem-arroba.com")]
        [InlineData("com@espaco .com")]
        public void CriarUsuario_ComEmailInvalido_DeveLancarArgumentException(string emailInvalido)
        {
            var nome = _faker.Person.FullName;
            var senhaHash = $"HASHED_{_faker.Internet.Password()}";
            var permissao = _faker.PickRandom<PermissaoEnum>();

            Action acao = () => new Usuario(nome, emailInvalido, senhaHash, permissao);

            acao.Should().Throw<ArgumentException>().WithMessage("*email*");
        }

        [Theory]
        [InlineData("")]
        [InlineData("    ")]
        public void CriarUsuario_ComSenhaHashInvalida_DeveLancarArgumentException(string senhaInvalida)
        {
            var nome = _faker.Person.FullName;
            var email = _faker.Internet.Email();
            var permissao = _faker.PickRandom<PermissaoEnum>();

            Action acao = () => new Usuario(nome, email, senhaInvalida, permissao);

            acao.Should().Throw<ArgumentException>().WithMessage("*senha*");
        }

        [Fact]
        public void CriarUsuario_ComPermissaoInvalida_DeveLancarArgumentException()
        {
            var nome = _faker.Person.FullName;
            var email = _faker.Internet.Email();
            var senhaHash = $"HASHED_{_faker.Internet.Password()}";
            var permissaoInvalida = (PermissaoEnum)999;

            Action acao = () => new Usuario(nome, email, senhaHash, permissaoInvalida);

            acao.Should().Throw<ArgumentException>().WithMessage("*Permissão inválida*");
        }

        [Fact]
        public void AtualizarInformacoesUsuario_DeveAtualizarEmailESenha()
        {
            var nome = _faker.Person.FullName;
            var emailOriginal = _faker.Internet.Email();
            var senhaHashOriginal = $"HASHED_{_faker.Internet.Password()}";
            var permissao = _faker.PickRandom<PermissaoEnum>();

            var usuario = new Usuario(nome, emailOriginal, senhaHashOriginal, permissao);

            var novoEmail = _faker.Internet.Email();
            var novaSenhaHash = $"HASHED_{_faker.Internet.Password()}";

            usuario.AtualizarInformacoesUsuario(novoEmail, novaSenhaHash);

            usuario.Email.Should().Be(novoEmail);
            usuario.SenhaHash.Should().Be(novaSenhaHash);
        }

        [Fact]
        public void AtualizarInformacoesUsuario_ComEmailInvalido_DeveLancarArgumentException()
        {
            var nome = _faker.Person.FullName;
            var emailOriginal = _faker.Internet.Email();
            var senhaHashOriginal = $"HASHED_{_faker.Internet.Password()}";
            var permissao = _faker.PickRandom<PermissaoEnum>();

            var usuario = new Usuario(nome, emailOriginal, senhaHashOriginal, permissao);

            Action acao = () => usuario.AtualizarInformacoesUsuario("email_invalido", "HASHED");

            acao.Should().Throw<ArgumentException>().WithMessage("*email*");
        }

        [Fact]
        public void AtualizarInformacoesUsuario_ComSenhaInvalida_DeveLancarArgumentException()
        {
            var nome = _faker.Person.FullName;
            var emailOriginal = _faker.Internet.Email();
            var senhaHashOriginal = $"HASHED_{_faker.Internet.Password()}";
            var permissao = _faker.PickRandom<PermissaoEnum>();

            var usuario = new Usuario(nome, emailOriginal, senhaHashOriginal, permissao);

            Action acao = () => usuario.AtualizarInformacoesUsuario(_faker.Internet.Email(), "");

            acao.Should().Throw<ArgumentException>().WithMessage("*senha*");
        }

        [Fact]
        public void AtualizarPermissaoUsuario_ComGerente_DeveAtualizar()
        {
            var nome = _faker.Person.FullName;
            var email = _faker.Internet.Email();
            var senhaHash = $"HASHED_{_faker.Internet.Password()}";
            var usuario = new Usuario(nome, email, senhaHash, PermissaoEnum.Operador);

            usuario.AtualizarPermissaoUsuario(PermissaoEnum.Gerente, PermissaoEnum.Gerente);

            usuario.Permissao.Should().Be(PermissaoEnum.Gerente);
        }

        [Fact]
        public void AtualizarPermissaoUsuario_ComNaoGerente_DeveLancarUnauthorizedAccessException()
        {
            var nome = _faker.Person.FullName;
            var email = _faker.Internet.Email();
            var senhaHash = $"HASHED_{_faker.Internet.Password()}";
            var usuario = new Usuario(nome, email, senhaHash, PermissaoEnum.Operador);

            Action acao = () => usuario.AtualizarPermissaoUsuario(PermissaoEnum.Gerente, PermissaoEnum.Operador);

            acao.Should().Throw<UnauthorizedAccessException>().WithMessage("*Gerentes*");
        }
    }
}
