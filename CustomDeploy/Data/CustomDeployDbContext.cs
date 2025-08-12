using Microsoft.EntityFrameworkCore;
using CustomDeploy.Models.Entities;
using CustomDeploy.Data.Configurations;

namespace CustomDeploy.Data
{
    public class CustomDeployDbContext : DbContext
    {
        public CustomDeployDbContext(DbContextOptions<CustomDeployDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<AcessoNivel> AcessoNiveis { get; set; }
        public DbSet<UsuarioAcesso> UsuarioAcessos { get; set; }
        public DbSet<Deploy> Deploys { get; set; }
        public DbSet<DeployComando> DeployComandos { get; set; }
        public DbSet<DeployHistorico> DeployHistoricos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar configurações
            modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
            modelBuilder.ApplyConfiguration(new AcessoNivelConfiguration());
            modelBuilder.ApplyConfiguration(new UsuarioAcessoConfiguration());
            modelBuilder.ApplyConfiguration(new DeployConfiguration());
            modelBuilder.ApplyConfiguration(new DeployComandoConfiguration());
            modelBuilder.ApplyConfiguration(new DeployHistoricoConfiguration());
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entityEntry in entries)
            {
                if (entityEntry.Entity is Usuario usuario)
                {
                    if (entityEntry.State == EntityState.Added)
                    {
                        usuario.CriadoEm = DateTime.UtcNow;
                    }
                    else if (entityEntry.State == EntityState.Modified)
                    {
                        usuario.AtualizadoEm = DateTime.UtcNow;
                    }
                }
                else if (entityEntry.Entity is AcessoNivel acessoNivel)
                {
                    if (entityEntry.State == EntityState.Added)
                    {
                        acessoNivel.CriadoEm = DateTime.UtcNow;
                    }
                    else if (entityEntry.State == EntityState.Modified)
                    {
                        acessoNivel.AtualizadoEm = DateTime.UtcNow;
                    }
                }
                else if (entityEntry.Entity is UsuarioAcesso usuarioAcesso)
                {
                    if (entityEntry.State == EntityState.Added)
                    {
                        usuarioAcesso.CriadoEm = DateTime.UtcNow;
                    }
                    else if (entityEntry.State == EntityState.Modified)
                    {
                        usuarioAcesso.AtualizadoEm = DateTime.UtcNow;
                    }
                }
                else if (entityEntry.Entity is Deploy deploy)
                {
                    if (entityEntry.State == EntityState.Added)
                    {
                        deploy.CriadoEm = DateTime.UtcNow;
                        if (deploy.Data == default)
                            deploy.Data = DateTime.UtcNow;
                    }
                    else if (entityEntry.State == EntityState.Modified)
                    {
                        deploy.AtualizadoEm = DateTime.UtcNow;
                    }
                }
                else if (entityEntry.Entity is DeployComando deployComando)
                {
                    if (entityEntry.State == EntityState.Added)
                    {
                        deployComando.CriadoEm = DateTime.UtcNow;
                    }
                    else if (entityEntry.State == EntityState.Modified)
                    {
                        deployComando.AtualizadoEm = DateTime.UtcNow;
                    }
                }
                else if (entityEntry.Entity is DeployHistorico deployHistorico)
                {
                    if (entityEntry.State == EntityState.Added)
                    {
                        deployHistorico.CriadoEm = DateTime.UtcNow;
                        if (deployHistorico.Data == default)
                            deployHistorico.Data = DateTime.UtcNow;
                    }
                    else if (entityEntry.State == EntityState.Modified)
                    {
                        deployHistorico.AtualizadoEm = DateTime.UtcNow;
                    }
                }
            }
        }
    }
}
