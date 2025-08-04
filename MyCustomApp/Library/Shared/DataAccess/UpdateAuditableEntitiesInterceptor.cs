using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Domain;
using Shared.Interfaces;

namespace Shared.DataAccess
{
    public sealed class UpdateAuditableEntitiesInterceptor(IUserIdentity userIdentity) : SaveChangesInterceptor
    {
        private readonly IUserIdentity _userIdentity = userIdentity;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default(CancellationToken))
        {
            DbContext context = eventData.Context!;
            if (context is null)
            {
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            foreach (EntityEntry<IUserEntity> item in context.ChangeTracker.Entries<IUserEntity>())
            {
                if (item.State is EntityState.Added && _userIdentity.UserId is not 0)
                {
                    item.Property((IUserEntity a) => a.UserId).CurrentValue = _userIdentity.UserId;
                }
            }

            if (_userIdentity.UserId is 0)
            {
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }

            DateTimeOffset utcNow = DateTimeOffset.UtcNow;
            foreach (EntityEntry<IAuditableEntity> item2 in context.ChangeTracker.Entries<IAuditableEntity>())
            {
                switch (item2.State)
                {
                    case EntityState.Added:
                        item2.Property((IAuditableEntity a) => a.CreatedOn).CurrentValue = utcNow;
                        item2.Property((IAuditableEntity a) => a.CreatedBy).CurrentValue = _userIdentity.UserId;
                        item2.Property((IAuditableEntity a) => a.ModifiedOn).CurrentValue = utcNow;
                        item2.Property((IAuditableEntity a) => a.ModifiedBy).CurrentValue = _userIdentity.UserId;
                        break;
                    case EntityState.Modified:
                        item2.Property((IAuditableEntity a) => a.ModifiedOn).CurrentValue = utcNow;
                        item2.Property((IAuditableEntity a) => a.ModifiedBy).CurrentValue = _userIdentity.UserId;
                        break;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
