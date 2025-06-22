using Microsoft.EntityFrameworkCore;
using UsuariosApp.Domain.InterfaceService;
using UsuariosApp.InfraStructure.Context;

namespace UsuariosApp.InfraStructure.UnidadesTrabalhos
{
    public class UnidadeTrabalho : IUnidadeTrabalho, IDisposable
    {
        private readonly UsuarioDbContext _usuarioDbContext;

        public UnidadeTrabalho(UsuarioDbContext usuarioDbContext)
        {
            _usuarioDbContext = usuarioDbContext;
        }

        public async Task CommitAsync()
        {
            await _usuarioDbContext.SaveChangesAsync();
        }

        public Task RollbackAsync()
        {
            foreach (var entry in _usuarioDbContext.ChangeTracker.Entries())
            {
                switch (entry.State)

                {

                    case EntityState.Modified:

                        entry.State = EntityState.Unchanged;

                        break;

                    case EntityState.Added:

                        entry.State = EntityState.Detached;

                        break;

                    case EntityState.Deleted:

                        entry.Reload();

                        break;

                }

            }

            return Task.CompletedTask;

        }

        public void Dispose()

        {

            _usuarioDbContext.Dispose();

        }
    }
}



