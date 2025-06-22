namespace UsuariosApp.Application.InterfaceSecurities
{
    public interface ISenhaHasher
    {
        string HashPassword(string senha);

        bool VerifyPassword(string senha, string senhaHash);
    }
}
