using System.Text.RegularExpressions;
using UsuariosApp.Domain.Enums;

namespace UsuariosApp.Domain.Entities
{

    public class Usuario
    {
        #region Propriedades da Entidade Usuario

        public Guid Id { get; private set; }
        public string? Nome { get; private set; }
        public string? Email { get; private set; }
        public string? SenhaHash { get; private set; }
        public DateTime DataCriacao { get; private set; }
        public PermissaoEnum Permissao { get; private set; }
        #endregion

        // Construtor público para inicializar um novo usuário
        public Usuario(
            string nome, string email, string senhaHash, PermissaoEnum permissao)
        {
            ValidarNome(nome);
            ValidarEmail(email);
            ValidarSenhaHash(senhaHash);
            ValidarPermissao(permissao);

            Id = Guid.NewGuid();
            Nome = nome;
            Email = email;
            SenhaHash = senhaHash;
            DataCriacao = DateTime.UtcNow;
            Permissao = permissao;
        }

        // Construtor privado para uso do entityFramework validar a proprieadade Nome do usuário, antes de persistir no banco de dados
        private void ValidarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome é obrigatório.", nameof(nome));

            if (nome.Length < 3 || nome.Length > 100)
                throw new ArgumentException("O nome deve ter entre 3 e 100 caracteres.", nameof(nome));

            // Apenas letras (incluindo acentos) e espaços
            if (!Regex.IsMatch(nome, @"^[A-Za-zÀ-ÿ\s]+$"))
                throw new ArgumentException("O nome deve conter apenas letras e espaços.", nameof(nome));
        }

        private void ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("O email é obrigatório.", nameof(email));

            // Simples validação básica de email
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("O email deve ser um endereço válido.", nameof(email));
        }

        private void ValidarSenhaHash(string senhaHash)
        {
            if (string.IsNullOrWhiteSpace(senhaHash))
                throw new ArgumentException("A senha (hash) é obrigatória.", nameof(senhaHash));
        }

        private void ValidarPermissao(PermissaoEnum permissao)
        {
            if (!Enum.IsDefined(typeof(PermissaoEnum), permissao))
                throw new ArgumentException("Permissão inválida. Valores válidos: Operador, Supervisor, Gerente.", nameof(permissao));
        }

        // Método para atualizar as permissões do usuário
        public void AtualizarPermissaoUsuario(PermissaoEnum novaPermissao, PermissaoEnum permissaoSolicitante)
        {
            if (permissaoSolicitante != PermissaoEnum.Gerente)
            {
                throw new UnauthorizedAccessException("Apenas Gerentes podem atualizar permissões.");
            }

            Permissao = novaPermissao;
        }

        public void AtualizarInformacoesUsuario(string novoEmail, string novaSenhaHash)
        {

            ValidarEmail(novoEmail);
            ValidarSenhaHash(novaSenhaHash);


            Email = novoEmail;
            SenhaHash = novaSenhaHash;

        }
    }
}