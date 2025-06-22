using Microsoft.EntityFrameworkCore;
using UsuariosApp.Domain.Entities;
using UsuariosApp.Domain.InterfaceRepository;
using UsuariosApp.InfraStructure.Context;

namespace UsuariosApp.InfraStructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {

        private readonly UsuarioDbContext _usuarioDbContext;

        public UsuarioRepository(UsuarioDbContext usuarioDbContext)
        {
            _usuarioDbContext = usuarioDbContext;
        }

        public async Task<Usuario> AdicionarAsync(Usuario usuario)
        {
            await _usuarioDbContext.Usuarios.AddAsync(usuario);
            return usuario;
        }

        public Task<Usuario> AtualizarAsync(Usuario usuario)
        {
            _usuarioDbContext.Usuarios.Update(usuario);
            return Task.FromResult(usuario);
        }

        public Task<List<Usuario>> ObterTodosUsuariosAsync()
        {
            return _usuarioDbContext.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.Nome)
                .ToListAsync();
        }

        public Task<Usuario?> ObterUsuarioPorIdAsync(Guid id)
        {
            return _usuarioDbContext.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> RemoverAsync(Guid id)
        {
            var usuario = await _usuarioDbContext.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return false;
            }

            _usuarioDbContext.Usuarios.Remove(usuario);

            return true;
        }

        public Task<bool> UsuarioExisteAsync(string email)
        {
            return _usuarioDbContext.Usuarios.AnyAsync(u => u.Email == email);
        }
    }
}
