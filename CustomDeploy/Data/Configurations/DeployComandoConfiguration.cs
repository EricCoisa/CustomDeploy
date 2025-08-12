using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CustomDeploy.Models.Entities;

namespace CustomDeploy.Data.Configurations
{
    public class DeployComandoConfiguration : IEntityTypeConfiguration<DeployComando>
    {
        public void Configure(EntityTypeBuilder<DeployComando> builder)
        {
            builder.ToTable("DeployComandos");

            builder.HasKey(dc => dc.Id);

            builder.Property(dc => dc.Id)
                .ValueGeneratedOnAdd();

            builder.Property(dc => dc.DeployId)
                .IsRequired();

            builder.Property(dc => dc.Comando)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(dc => dc.Ordem)
                .IsRequired();

            builder.Property(dc => dc.CriadoEm)
                .IsRequired()
                .HasDefaultValueSql("datetime('now')");

            builder.Property(dc => dc.AtualizadoEm)
                .IsRequired(false);

            // Relacionamentos
            builder.HasOne(dc => dc.Deploy)
                .WithMany(d => d.DeployComandos)
                .HasForeignKey(dc => dc.DeployId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(dc => dc.DeployId)
                .HasDatabaseName("IX_DeployComando_DeployId");

            builder.HasIndex(dc => new { dc.DeployId, dc.Ordem })
                .IsUnique()
                .HasDatabaseName("IX_DeployComando_DeployId_Ordem");
        }
    }
}
