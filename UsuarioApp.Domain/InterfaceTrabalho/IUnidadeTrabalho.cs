namespace UsuariosApp.Domain.InterfaceService
{
    public interface IUnidadeTrabalho
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
}
