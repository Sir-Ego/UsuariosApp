using Bogus;
using FluentAssertions;
using UsuariosApp.Application.Dtos;
using UsuariosApp.Application.Validations;
using UsuariosApp.Domain.Enums;

namespace UsuariosApp.Application.Tests.TestesValidator
{
    public class UsuarioValidatorTests
    {
        private readonly UsuarioValidator _usuarioValidator;
        private readonly Faker<UsuarioRequestDto> _faker;

        public UsuarioValidatorTests()
        {
            _usuarioValidator = new UsuarioValidator();

            _faker = new Faker<UsuarioRequestDto>("pt_BR")
                .RuleFor(u => u.Nome, f => f.Person.FullName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Senha, f =>  "Senha@123")
                .RuleFor(u => u.Permissao, f => f.PickRandom<PermissaoEnum>());
        }

        [Fact]
        public void Deve_Passar_Validacao_Quando_Dados_Forem_Validos()
        {
            var usuarioRequestDto = _faker.Generate();

            var resultado = _usuarioValidator.Validate(usuarioRequestDto);

            resultado.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]                           // Teste Nome em branco
        [InlineData("Jo")]                         // Teste Nome muito curto
        [InlineData("João123!")]                   // Teste Nome com caracteres inválidos
        public void Deve_Falhar_Quando_Nome_For_Invalido(string nomeInvalido)
        {
            var usuarioRequestDto = _faker.Generate();
            usuarioRequestDto.Nome = nomeInvalido;

            var resultado = _usuarioValidator.Validate(usuarioRequestDto);

            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().Contain(x => x.PropertyName == "Nome");
        }

        [Fact]
        public void Deve_Falhar_Quando_Nome_Tiver_Mais_De_100_Caracteres()
        {
            var usuarioRequestDto = _faker.Generate();
            usuarioRequestDto.Nome = new string('A', 101);

            var resultado = _usuarioValidator.Validate(usuarioRequestDto);

            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().Contain(x => x.PropertyName == "Nome");
        }


        [Theory]
        [InlineData("")]               // Teste Email em branco
        [InlineData("emailinvalido")]  // Teste Email inválido sem @
        [InlineData("teste@")]         // Teste Email inválido sem dominio
        [InlineData("teste@.com")]     // Teste Email inválido dominio incompleto
        public void Deve_Falhar_Quando_Email_For_Invalido(string emailInvalido)
        {
            var usuarioRequestDto = _faker.Generate();
            usuarioRequestDto.Email = emailInvalido;

            var resultado = _usuarioValidator.Validate(usuarioRequestDto);

            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().Contain(x => x.PropertyName == "Email");
        }

        [Theory]
        [InlineData("")]             // Teste Senha em branco
        [InlineData("senha123")]     // Teste Senha sem maiúscula e símbolo
        [InlineData("SENHA123")]     // Teste Senha sem minúscula e símbolo
        [InlineData("Senha123")]     // Teste Senha sem símbolo
        [InlineData("Senha!")]       // Teste Senha sem número
        public void Deve_Falhar_Quando_Senha_For_Fraca(string senhaInvalida)
        {
            var usuarioRequestDto = _faker.Generate();
            usuarioRequestDto.Senha = senhaInvalida;

            var resultado = _usuarioValidator.Validate(usuarioRequestDto);

            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().Contain(x => x.PropertyName == "Senha");
        }

        [Fact]
        public void Deve_Falhar_Quando_Permissao_For_Invalida()
        {
            var usuarioRequestDto = _faker.Generate();

            usuarioRequestDto.Permissao = (PermissaoEnum)999;

            var resultado = _usuarioValidator.Validate(usuarioRequestDto);

            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().Contain(x => x.PropertyName == "Permissao");
        }
    }
}
