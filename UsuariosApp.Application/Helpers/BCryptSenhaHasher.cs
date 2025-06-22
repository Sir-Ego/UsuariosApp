using UsuariosApp.Application.InterfaceSecurities;

namespace UsuariosApp.Application.Helpers
{
    public class BCryptSenhaHasher : ISenhaHasher
    {
        public string HashPassword(string senha)
        {
            if (senha == null) 
                throw new ArgumentNullException(nameof(senha));

            return BCrypt.Net.BCrypt.HashPassword(senha);
        }
        public bool VerifyPassword(string senha, string senhaHash)
        {
            if (senha == null) 
                throw new ArgumentNullException(nameof(senha));

            if (senhaHash == null)
                throw new ArgumentNullException(nameof(senhaHash));
            
            return BCrypt.Net.BCrypt.Verify(senha, senhaHash);
        }
    }

}
