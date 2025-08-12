using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CustomDeploy.Models.Entities;

namespace CustomDeploy.Data.Configurations
{
    public class DeployHistoricoConfiguration : IEntityTypeConfiguration<DeployHistorico>
    {
        public void Configure(EntityTypeBuilder<DeployHistorico> builder)
        {
            builder.ToTable("DeployHistoricos");

            builder.HasKey(dh => dh.Id);

            builder.Property(dh => dh.Id)
                .ValueGeneratedOnAdd();

            builder.Property(dh => dh.DeployId)
                .IsRequired();

            builder.Property(dh => dh.Data)
                .IsRequired()
                .HasDefaultValueSql("datetime('now')");

            builder.Property(dh => dh.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(dh => dh.Mensagem)
                .HasMaxLength(2000);

            builder.Property(dh => dh.CriadoEm)
                .IsRequired()
                .HasDefaultValueSql("datetime('now')");

            builder.Property(dh => dh.AtualizadoEm)
                .IsRequired(false);

            // Relacionamentos
            builder.HasOne(dh => dh.Deploy)
                .WithMany(d => d.DeployHistoricos)
                .HasForeignKey(dh => dh.DeployId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(dh => dh.DeployId)
                .HasDatabaseName("IX_DeployHistorico_DeployId");

            builder.HasIndex(dh => dh.Data)
                .HasDatabaseName("IX_DeployHistorico_Data");

            builder.HasIndex(dh => dh.Status)
                .HasDatabaseName("IX_DeployHistorico_Status");
        }
    }
}
