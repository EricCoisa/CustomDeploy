using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CustomDeploy.Models.Entities;

namespace CustomDeploy.Data.Configurations
{
    public class AcessoNivelConfiguration : IEntityTypeConfiguration<AcessoNivel>
    {
        public void Configure(EntityTypeBuilder<AcessoNivel> builder)
        {
            builder.ToTable("AcessoNiveis");

            builder.HasKey(an => an.Id);

            builder.Property(an => an.Id)
                .ValueGeneratedOnAdd();

            builder.Property(an => an.Nome)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(an => an.CriadoEm)
                .IsRequired()
                .HasDefaultValueSql("datetime('now')");

            builder.Property(an => an.AtualizadoEm)
                .IsRequired(false);

            // Relacionamentos
            builder.HasMany(an => an.UsuarioAcessos)
                .WithOne(ua => ua.AcessoNivel)
                .HasForeignKey(ua => ua.AcessoNivelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(an => an.Nome)
                .IsUnique()
                .HasDatabaseName("IX_AcessoNivel_Nome");

            // Dados iniciais
            builder.HasData(
                new AcessoNivel { Id = 1, Nome = "Administrador", CriadoEm = DateTime.UtcNow },
                new AcessoNivel { Id = 2, Nome = "Operador", CriadoEm = DateTime.UtcNow }
            );
        }
    }
}
