using Bogus;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using UsuariosApp.Application.Dtos;
using UsuariosApp.Application.InterfaceSecurities;
using UsuariosApp.Application.Services;
using UsuariosApp.Domain.Entities;
using UsuariosApp.Domain.Enums;
using UsuariosApp.Domain.InterfaceRepository;
using UsuariosApp.Domain.InterfaceService;


namespace UsuariosApp.Application.Tests.TestesService
{
    public class UsuarioServiceTests
    {
        private readonly UsuarioService _usuarioService;
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<ISenhaHasher> _senhaHasherMock;
        private readonly Mock<IUnidadeTrabalho> _unidadeTrabalhoMock;
        private readonly Mock<IValidator<UsuarioRequestDto>> _usuarioValidatorMock;
        private readonly Faker<UsuarioRequestDto> _faker;

        public UsuarioServiceTests()
        {
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _senhaHasherMock = new Mock<ISenhaHasher>();
            _unidadeTrabalhoMock = new Mock<IUnidadeTrabalho>();
            _usuarioValidatorMock = new Mock<IValidator<UsuarioRequestDto>>();

            _usuarioService = new UsuarioService(
                _usuarioRepositoryMock.Object,
                _senhaHasherMock.Object,
                _unidadeTrabalhoMock.Object,
               _usuarioValidatorMock.Object
            );

            _faker = new Faker<UsuarioRequestDto>("pt_BR")
                .RuleFor(u => u.Nome, f => f.Person.FullName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Senha, f => f.Internet.Password())
                .RuleFor(u => u.Permissao, f => f.PickRandom<PermissaoEnum>());
        }

        [Fact]
        public async Task CreateUsuarioAsync_DeveCriarUsuarioComSucesso()
        {
            var usuarioRequestDto = _faker.Generate();
            var senhaHash = $"HASHED_{usuarioRequestDto.Senha}";


            _usuarioValidatorMock.Setup(v => v.ValidateAsync(usuarioRequestDto, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _usuarioRepositoryMock.Setup(r => r.UsuarioExisteAsync(usuarioRequestDto.Email!))
                .ReturnsAsync(false);

            _senhaHasherMock.Setup(h => h.HashPassword(usuarioRequestDto.Senha!))
                .Returns(senhaHash);

            var result = await _usuarioService.CreateUsuarioAsync(usuarioRequestDto);

            result.Should().NotBeNull();
            result.Nome.Should().Be(usuarioRequestDto.Nome);
            result.Email.Should().Be(usuarioRequestDto.Email);
            result.Permissao.Should().Be(usuarioRequestDto.Permissao.ToString());
            result.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CreateUsuarioAsync_DeveLancarValidationException_QuandoValidacaoFalhar()
        {
            var usuarioRequestDto = _faker.Generate();
            var erros = new List<ValidationFailure>
            {
                new("Email", "Email inválido")
            };

            _usuarioValidatorMock.Setup(v => v.ValidateAsync(usuarioRequestDto, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(erros));

            var acao = async () => await _usuarioService.CreateUsuarioAsync(usuarioRequestDto);

            await acao.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Email inválido*");
        }

        [Fact]
        public async Task CreateUsuarioAsync_DeveLancarArgumentException_QuandoEmailJaExistir()
        {
            var usuarioRequestDto = _faker.Generate();

            _usuarioValidatorMock.Setup(v => v.ValidateAsync(usuarioRequestDto, default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            _usuarioRepositoryMock.Setup(r => r.UsuarioExisteAsync(usuarioRequestDto.Email!))
                .ReturnsAsync(true);

            var acao = async () => await _usuarioService.CreateUsuarioAsync(usuarioRequestDto);

            await acao.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Já existe um usuário com este email*");
        }

        [Fact]
        public async Task DeleteUsuarioAsync_DeveExcluirComSucesso()
        {
            var id = Guid.NewGuid();
            var usuario = new Usuario("Teste", "teste@email.com", "hash", PermissaoEnum.Gerente);

            _usuarioRepositoryMock.Setup(r => r.ObterUsuarioPorIdAsync(id)).ReturnsAsync(usuario);
            _usuarioRepositoryMock.Setup(r => r.RemoverAsync(id)).ReturnsAsync(true);

            await _usuarioService.DeleteUsuarioAsync(id);

            _usuarioRepositoryMock.Verify(r => r.RemoverAsync(id), Times.Once);
            _unidadeTrabalhoMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteUsuarioAsync_DeveLancarExcecao_QuandoUsuarioNaoEncontrado()
        {
            var id = Guid.NewGuid();
            _usuarioRepositoryMock.Setup(r => r.ObterUsuarioPorIdAsync(id)).ReturnsAsync((Usuario)null!);

            var acao = async () => await _usuarioService.DeleteUsuarioAsync(id);

            await acao.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Usuário com ID {id} não encontrado.");
        }

        [Fact]
        public async Task GetUsuarioByIdAsync_DeveRetornarComSucesso()
        {
            var id = Guid.NewGuid();
            var usuario = new Usuario("Usuário", "teste@email.com", "hash", PermissaoEnum.Supervisor);

            _usuarioRepositoryMock.Setup(r => r.ObterUsuarioPorIdAsync(id)).ReturnsAsync(usuario);

            var result = await _usuarioService.GetUsuarioByIdAsync(id);

            result.Should().NotBeNull();
            result.Email.Should().Be(usuario.Email);
        }

        [Fact]
        public async Task GetUsuarioByIdAsync_DeveLancarExcecao_QuandoNaoEncontrado()
        {
            var id = Guid.NewGuid();
            _usuarioRepositoryMock.Setup(r => r.ObterUsuarioPorIdAsync(id)).ReturnsAsync((Usuario)null!);

            var acao = async () => await _usuarioService.GetUsuarioByIdAsync(id);

            await acao.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Usuário com ID {id} não encontrado.");
        }

        [Fact]
        public async Task GetAllUsuariosAsync_DeveRetornarListaDeUsuarios()
        {
            var dtoList = _faker.Generate(5);

            var usuarios = dtoList.Select(dto =>
                new Usuario(
                    dto.Nome!,
                    dto.Email!,
                    $"HASHED_{dto.Senha}",
                    dto.Permissao
                )
            ).ToList();

            _usuarioRepositoryMock
                .Setup(r => r.ObterTodosUsuariosAsync())
                .ReturnsAsync(usuarios);

            var result = await _usuarioService.GetAllUsuariosAsync();

            result.Should().NotBeEmpty().And.HaveCount(5);
        }

        [Fact]
        public async Task GetAllUsuariosAsync_DeveRetornarListaVazia_SeNaoExistirUsuarios()
        {
            _usuarioRepositoryMock.Setup(r => r.ObterTodosUsuariosAsync()).ReturnsAsync(new List<Usuario>());

            var result = await _usuarioService.GetAllUsuariosAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateUsuarioAsync_DeveAtualizarComSucesso()
        {
            var id = Guid.NewGuid();
            var usuarioRequestDto = _faker.Generate();
            var senhaHash = $"HASHED_{usuarioRequestDto.Senha}";

            _senhaHasherMock
                .Setup(h => h.HashPassword(usuarioRequestDto.Senha!))
                .Returns(senhaHash);

            var usuario = new Usuario(usuarioRequestDto.Nome!, usuarioRequestDto.Email!, senhaHash, usuarioRequestDto.Permissao);


            _usuarioRepositoryMock.Setup(r => r.ObterUsuarioPorIdAsync(id)).ReturnsAsync(usuario);
            _usuarioValidatorMock.Setup(v => v.ValidateAsync(usuarioRequestDto, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());

            var result = await _usuarioService.UpdateUsuarioAsync(usuarioRequestDto, id);

            result.Should().NotBeNull();
            result.Email.Should().Be(usuarioRequestDto.Email);
        }

        [Fact]
        public async Task UpdateUsuarioAsync_DeveLancar_SeUsuarioNaoExistir()
        {
            var id = Guid.NewGuid();
            var usuarioRequestDto = _faker.Generate();

            _usuarioRepositoryMock.Setup(r => r.ObterUsuarioPorIdAsync(id)).ReturnsAsync((Usuario)null!);

            var acao = async () => await _usuarioService.UpdateUsuarioAsync(usuarioRequestDto, id);

            await acao.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage($"Usuário com ID {id} não encontrado.");
        }

        [Fact]
        public async Task UpdateUsuarioAsync_DeveLancar_SeValidacaoFalhar()
        {
            var id = Guid.NewGuid();
            var usuarioRequestDto = _faker.Generate();

            var senhaHash = $"HASHED_{usuarioRequestDto.Senha}";
            _senhaHasherMock.Setup(h => h.HashPassword(usuarioRequestDto.Senha!)).Returns(senhaHash);

            var usuario = new Usuario(usuarioRequestDto.Nome!, usuarioRequestDto.Email!, senhaHash, usuarioRequestDto.Permissao);


            var erros = new List<ValidationFailure> { new("Senha", "Senha fraca") };

            _usuarioRepositoryMock.Setup(r => r.ObterUsuarioPorIdAsync(id)).ReturnsAsync(usuario);
            _usuarioValidatorMock.Setup(v => v.ValidateAsync(usuarioRequestDto, default)).ReturnsAsync(new FluentValidation.Results.ValidationResult(erros));

            var acao = async () => await _usuarioService.UpdateUsuarioAsync(usuarioRequestDto, id);

            await acao.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Senha fraca*");
        }
    }
}
