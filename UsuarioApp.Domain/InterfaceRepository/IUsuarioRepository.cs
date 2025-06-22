using UsuariosApp.Domain.Entities;

namespace UsuariosApp.Domain.InterfaceRepository
{
    public interface IUsuarioRepository
    {
        Task<bool> UsuarioExisteAsync(string email);
        Task<Usuario?> ObterUsuarioPorIdAsync(Guid id);
        Task<List<Usuario>> ObterTodosUsuariosAsync();
        Task<Usuario> AdicionarAsync(Usuario usuario);
        Task<Usuario> AtualizarAsync(Usuario usuario);
        Task<bool> RemoverAsync(Guid id);
    }
}
