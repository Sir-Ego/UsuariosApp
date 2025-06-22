using Microsoft.EntityFrameworkCore;
using UsuariosApp.Domain.Entities;

namespace UsuariosApp.InfraStructure.Context
{
    public class UsuarioDbContext : DbContext
    {

        public UsuarioDbContext(DbContextOptions<UsuarioDbContext> options)
        : base(options)
        {
        }

        // Configuração da connection string e outras opções do DbContext
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Configuração adicional do DbContext, se necessário
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("SuaStringDeConexaoAqui"); // Substitua pela sua string de conexão
            }
        }

        // DbSet para a entidade Usuario, que representa a tabela de usuários no banco de dados
        public DbSet<Usuario> Usuarios { get; set; }

        // Método para configurar o modelo do banco de dados
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsuarioDbContext).Assembly);
        }
    }

}
