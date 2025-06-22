using Bogus;
using FluentAssertions;
using UsuariosApp.Application.Dtos;
using UsuariosApp.Application.Helpers;
using UsuariosApp.Application.InterfaceSecurities;

namespace UsuariosApp.Application.Tests.TestesCriptografia
{
    public class SenhaCriptografiaTests
    {
        private readonly ISenhaHasher _senhaHasher;
        private readonly Faker<UsuarioRequestDto> _faker;

        public SenhaCriptografiaTests()
        {
            _senhaHasher = new BCryptSenhaHasher();

            _faker = new Faker<UsuarioRequestDto>("pt_BR")
                .RuleFor(u => u.Senha, f => "Senha@123"); 
        }

        [Theory]
        [InlineData("Senha@123")]
        [InlineData("OutraSenha#456")]
        [InlineData("SenhaForte!789")]
        public void Deve_Gerar_Hash_Diferente_da_Senha(string senhaSegura)
        {
            var hash = _senhaHasher.HashPassword(senhaSegura);

            hash.Should().NotBeNullOrEmpty();
            hash.Should().NotBe(senhaSegura);
        }

        [Theory]
        [InlineData("Senha@123")]
        [InlineData("OutraSenha#456")]
        public void Deve_Validar_Senha_Correta(string senhaOriginal)
        {
            var hash = _senhaHasher.HashPassword(senhaOriginal);

            var resultado = _senhaHasher.VerifyPassword(senhaOriginal, hash);

            resultado.Should().BeTrue("A senha correta deve ser validada com sucesso");
        }

        [Fact]
        public void Nao_Deve_Validar_Senha_Incorreta()
        {
            var senhaOriginal = "Senha@123";
            var senhaErrada = "SenhaErrada@123";

            var hash = _senhaHasher.HashPassword(senhaOriginal);

            var resultado = _senhaHasher.VerifyPassword(senhaErrada, hash);

            resultado.Should().BeFalse("Senhas diferentes não devem ser validadas");
        }

        [Fact]
        public void HashPassword_Deve_Lancar_Excecao_Quando_Senha_For_Nula()
        {
            Action act = () => _senhaHasher.HashPassword(null!);

            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("senha");
        }

        [Fact]
        public void VerifyPassword_Deve_Lancar_Excecao_Quando_Senha_For_Nula()
        {
            var hash = _senhaHasher.HashPassword("Senha@123");

            Action act = () => _senhaHasher.VerifyPassword(null!, hash);

            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("senha");
        }

        [Fact]
        public void VerifyPassword_Deve_Lancar_Excecao_Quando_Hash_For_Nulo()
        {
            var senha = "Senha@123";

            Action act = () => _senhaHasher.VerifyPassword(senha, null!);

            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("senhaHash");
        }
    }
}
