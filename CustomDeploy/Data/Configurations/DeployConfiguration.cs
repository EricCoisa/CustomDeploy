using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CustomDeploy.Models.Entities;

namespace CustomDeploy.Data.Configurations
{
    public class DeployConfiguration : IEntityTypeConfiguration<Deploy>
    {
        public void Configure(EntityTypeBuilder<Deploy> builder)
        {
            builder.ToTable("Deploys");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .ValueGeneratedOnAdd();

            builder.Property(d => d.SiteName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(d => d.ApplicationName)
                .HasMaxLength(200);

            builder.Property(d => d.Data)
                .IsRequired()
                .HasDefaultValueSql("datetime('now')");

            builder.Property(d => d.UsuarioId)
                .IsRequired();

            builder.Property(d => d.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.Mensagem)
                .HasMaxLength(2000);

            builder.Property(d => d.Plataforma)
                .HasMaxLength(100);

            builder.Property(d => d.CriadoEm)
                .IsRequired()
                .HasDefaultValueSql("datetime('now')");

            builder.Property(d => d.AtualizadoEm)
                .IsRequired(false);

            builder.Property(d => d.RepoUrl)
                .HasMaxLength(200);

            builder.Property(d => d.Branch)
                .HasMaxLength(100);

            builder.Property(d => d.BuildOutput)
                .HasMaxLength(100);

            // Relacionamentos
            builder.HasOne(d => d.Usuario)
                .WithMany(u => u.Deploys)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(d => d.DeployComandos)
                .WithOne(dc => dc.Deploy)
                .HasForeignKey(dc => dc.DeployId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.DeployHistoricos)
                .WithOne(dh => dh.Deploy)
                .HasForeignKey(dh => dh.DeployId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(d => d.SiteName)
                .HasDatabaseName("IX_Deploy_SiteName");

            builder.HasIndex(d => d.UsuarioId)
                .HasDatabaseName("IX_Deploy_UsuarioId");

            builder.HasIndex(d => d.Data)
                .HasDatabaseName("IX_Deploy_Data");

            builder.HasIndex(d => d.Status)
                .HasDatabaseName("IX_Deploy_Status");
        }
    }
}
