using MyCustomApp.Application.Contracts;
using MyCustomApp.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Shared.Interfaces;

namespace MyCustomApp.Infrastructure.Modules.Game
{
   
    public class GameDbContext(DbContextOptions<GameDbContext> options, IUserIdentity userIdentity) : DbContext(options), IGameDbContext
    {
        public DbSet<Domain.Modules.Game.Game> Games { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // Apply configurations
            modelBuilder.ApplyConfigurationsFromAssembly(
                       typeof(GameDbContext).Assembly,
                       t => t.Namespace != null && t.Namespace.Contains("Modules.Game.Configurations")
            );

            // Default Schema
            modelBuilder.HasDefaultSchema(SchemaConstants.Game);

            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetDefaultTableName());
                entity.SetHistoryTableName(entity.GetDefaultTableName());
                entity.SetHistoryTableSchema($"{SchemaConstants.Game}_HT");
                entity.SetIsTemporal(true);
                entity.GetForeignKeys()
                    .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade)
                    .ToList()
                    .ForEach(fk => fk.DeleteBehavior = DeleteBehavior.Restrict);
            }

        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
