using FluentValidation;
using UsuariosApp.Application.Dtos;
using UsuariosApp.Application.InterfaceSecurities;
using UsuariosApp.Application.InterfaceService;
using UsuariosApp.Domain.Entities;
using UsuariosApp.Domain.InterfaceRepository;
using UsuariosApp.Domain.InterfaceService;


namespace UsuariosApp.Application.Services
{
    public class UsuarioService : IUsuarioService
    {

        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ISenhaHasher _bCryptSenhaHasher;
        private readonly IUnidadeTrabalho _unidadeTrabalho;
        private readonly IValidator<UsuarioRequestDto> _usuarioValidator;

        public UsuarioService(
            IUsuarioRepository usuarioRepository, 
            ISenhaHasher bCryptSenhaHasher, 
            IUnidadeTrabalho unidadeTrabalho,
            IValidator<UsuarioRequestDto> usuarioValidator)
        {
            _usuarioRepository = usuarioRepository;
            _bCryptSenhaHasher = bCryptSenhaHasher;
            _unidadeTrabalho = unidadeTrabalho;
            _usuarioValidator = usuarioValidator;
        }

        public async Task<UsuarioResponseDto> CreateUsuarioAsync(UsuarioRequestDto dto)
        {

            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "Os dados do usuário não podem ser nulos.");
         
            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("O email é obrigatório.", nameof(dto.Email));

            if (string.IsNullOrWhiteSpace(dto.Senha))
                throw new ArgumentException("A senha é obrigatória.", nameof(dto.Senha));

            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new ArgumentException("O nome é obrigatório.", nameof(dto.Nome));

            var validationResult = await _usuarioValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            if (await _usuarioRepository.UsuarioExisteAsync(dto.Email))
                throw new ArgumentException("Já existe um usuário com este email.");

            var senhaHash = _bCryptSenhaHasher.HashPassword(dto.Senha);

            var usuario = new Usuario(dto.Nome, dto.Email, senhaHash, dto.Permissao);

            await _usuarioRepository.AdicionarAsync(usuario);
            await _unidadeTrabalho.CommitAsync();

            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Permissao = usuario.Permissao.ToString()
            };

        }

        public async Task DeleteUsuarioAsync(Guid id)
        {
            var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(id);

            if (usuario == null)
                throw new KeyNotFoundException($"Usuário com ID {id} não encontrado.");

            try
            {
                var sucessoRemocao = await _usuarioRepository.RemoverAsync(id);

                if (!sucessoRemocao)
                    throw new InvalidOperationException("Erro ao tentar remover o usuário.");

                await _unidadeTrabalho.CommitAsync();

            }
            catch
            {
                await _unidadeTrabalho.RollbackAsync();
                throw;
            }

        }

        public async Task<IEnumerable<UsuarioResponseDto>> GetAllUsuariosAsync()
        {
            var usuarios = await _usuarioRepository.ObterTodosUsuariosAsync();

            if (usuarios == null || !usuarios.Any())
                return Enumerable.Empty<UsuarioResponseDto>();

            return usuarios.Select(u => new UsuarioResponseDto
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                Permissao = u.Permissao.ToString(),
                DataCriacao = u.DataCriacao
            });

        }

        public async Task<UsuarioResponseDto> GetUsuarioByIdAsync(Guid id)
        {
            var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(id);

            if (usuario == null)
                throw new KeyNotFoundException($"Usuário com ID {id} não encontrado.");

            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Permissao = usuario.Permissao.ToString(),
                DataCriacao = usuario.DataCriacao
            };
        }

        public async Task<UsuarioResponseDto> UpdateUsuarioAsync(UsuarioRequestDto dto, Guid id)
        {
            // Busca o usuário no repositório
            var usuario = await _usuarioRepository.ObterUsuarioPorIdAsync(id);

            if (usuario == null)
                throw new KeyNotFoundException($"Usuário com ID {id} não encontrado.");

            // Verifica se o DTO contém email e/ou senha para atualização
            if (string.IsNullOrWhiteSpace(dto.Email) && string.IsNullOrWhiteSpace(dto.Senha))
                throw new ArgumentException("Informe ao menos o email ou a senha para atualização.");

            // Mantém a senha atual, a menos que o DTO tenha uma nova senha
            string novaSenhaHash = usuario.SenhaHash!;
            if (!string.IsNullOrWhiteSpace(dto.Senha))
                novaSenhaHash = _bCryptSenhaHasher.HashPassword(dto.Senha);

            // Atualiza email e senha (internamente valida os dados)
            usuario.AtualizarInformacoesUsuario(dto.Email ?? usuario.Email!, novaSenhaHash);

            // Valida os dados com FluentValidation
            var resultado = await _usuarioValidator.ValidateAsync(dto);
            if (!resultado.IsValid)
                throw new ValidationException(resultado.Errors);

            await _usuarioRepository.AtualizarAsync(usuario);
            await _unidadeTrabalho.CommitAsync();

            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Permissao = usuario.Permissao.ToString(),
                DataCriacao = usuario.DataCriacao
            };
        }


    }
}
