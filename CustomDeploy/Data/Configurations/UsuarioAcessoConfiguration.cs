using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CustomDeploy.Models.Entities;

namespace CustomDeploy.Data.Configurations
{
    public class UsuarioAcessoConfiguration : IEntityTypeConfiguration<UsuarioAcesso>
    {
        public void Configure(EntityTypeBuilder<UsuarioAcesso> builder)
        {
            builder.ToTable("UsuarioAcessos");

            builder.HasKey(ua => ua.Id);

            builder.Property(ua => ua.Id)
                .ValueGeneratedOnAdd();

            builder.Property(ua => ua.UsuarioId)
                .IsRequired();

            builder.Property(ua => ua.AcessoNivelId)
                .IsRequired();

            builder.Property(ua => ua.CriadoEm)
                .IsRequired()
                .HasDefaultValueSql("datetime('now')");

            builder.Property(ua => ua.AtualizadoEm)
                .IsRequired(false);

            // Relacionamentos
            builder.HasOne(ua => ua.Usuario)
                .WithOne(u => u.UsuarioAcesso)
                .HasForeignKey<UsuarioAcesso>(ua => ua.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ua => ua.AcessoNivel)
                .WithMany(an => an.UsuarioAcessos)
                .HasForeignKey(ua => ua.AcessoNivelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(ua => ua.UsuarioId)
                .IsUnique()
                .HasDatabaseName("IX_UsuarioAcesso_UsuarioId");

            builder.HasIndex(ua => ua.AcessoNivelId)
                .HasDatabaseName("IX_UsuarioAcesso_AcessoNivelId");
        }
    }
}
