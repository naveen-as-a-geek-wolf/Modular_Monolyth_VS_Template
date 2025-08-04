
using MyCustomApp.Application.Contracts;
using MyCustomApp.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shared.Interfaces;
namespace MyCustomApp.Infrastructure.Modules.User
{

    public class UserDbContext(DbContextOptions<UserDbContext> options, IUserIdentity userIdentity) : DbContext(options), IUserDbContext
    {
        public DbSet<Domain.Modules.User.User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // Apply configurations
            modelBuilder.ApplyConfigurationsFromAssembly(
                       typeof(UserDbContext).Assembly,
                       t => t.Namespace != null && t.Namespace.Contains("Modules.User.Configurations")
            );

            // Default Schema
            modelBuilder.HasDefaultSchema(SchemaConstants.User);

            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetDefaultTableName());
                entity.SetHistoryTableName(entity.GetDefaultTableName());
                entity.SetHistoryTableSchema($"{SchemaConstants.User}_HT");
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
