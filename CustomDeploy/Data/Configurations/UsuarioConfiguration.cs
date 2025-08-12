using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CustomDeploy.Models.Entities;

namespace CustomDeploy.Data.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .ValueGeneratedOnAdd();

            builder.Property(u => u.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.Senha)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(u => u.CriadoEm)
                .IsRequired()
                .HasDefaultValueSql("datetime('now')");

            builder.Property(u => u.AtualizadoEm)
                .IsRequired(false);

            // Relacionamentos
            builder.HasOne(u => u.UsuarioAcesso)
                .WithOne(ua => ua.Usuario)
                .HasForeignKey<UsuarioAcesso>(ua => ua.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.Deploys)
                .WithOne(d => d.Usuario)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Usuario_Email");
        }
    }
}
