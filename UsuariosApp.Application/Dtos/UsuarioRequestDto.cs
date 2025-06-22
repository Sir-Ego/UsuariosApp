using UsuariosApp.Domain.Enums;

namespace UsuariosApp.Application.Dtos
{
    public class UsuarioRequestDto
    {
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Senha { get; set; }
        public PermissaoEnum Permissao { get; set; }
    }
}

