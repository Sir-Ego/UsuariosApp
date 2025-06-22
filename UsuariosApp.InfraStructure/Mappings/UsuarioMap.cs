using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UsuariosApp.Domain.Entities;

namespace UsuariosApp.InfraStructure.Mappings
{
    public class UsuarioMap : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("USUARIOS");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasColumnName("ID");

            builder.Property(u => u.Nome)
                .HasColumnName("NOME")
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .HasColumnName("EMAIL")
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.SenhaHash)
                .HasColumnName("SENHA_HASH")
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.DataCriacao)
                .HasColumnName("DATA_CRIACAO")
                .IsRequired();    
            
            builder.Property(u => u.Permissao)
                .HasColumnName("PERMISSAO")
                .IsRequired()
                .HasConversion<int>(); 

        }
    }

}



