using BuildingBlocks.Common.DDD;
using BuildingBlocks.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Common.Interceptors
{
    public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUser _currentUser;

        public AuditableEntityInterceptor(ICurrentUser currentUser)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateEntities(DbContext? context)
        {
            if (context == null) return;

            var now = DateTime.UtcNow;
            var actor = GetCurrentActor();

            foreach (var entry in context.ChangeTracker.Entries<IEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property(nameof(IEntity.CreatedBy)).CurrentValue ??= actor;
                    entry.Property(nameof(IEntity.CreatedAt)).CurrentValue = now;
                }

                else if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                {
                    entry.Property(nameof(IEntity.UpdatedBy)).CurrentValue = actor;
                    entry.Property(nameof(IEntity.UpdatedAt)).CurrentValue = now;
                }

                else if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Property(nameof(IEntity.IsActive)).CurrentValue = false;
                    entry.Property(nameof(IEntity.IsDeleted)).CurrentValue = true;
                    entry.Property(nameof(IEntity.UpdatedBy)).CurrentValue = actor;
                    entry.Property(nameof(IEntity.UpdatedAt)).CurrentValue = now;

                    // Owned entity'leri (ValueObject) Unchanged yap,
                    // yoksa EF Core UPDATE'te bu kolonları NULL yapar.
                    foreach (var reference in entry.References)
                    {
                        if (reference.TargetEntry is { } ownedEntry
                            && ownedEntry.Metadata.IsOwned()
                            && ownedEntry.State == EntityState.Deleted)
                        {
                            ownedEntry.State = EntityState.Unchanged;
                        }
                    }
                }
            }
        }
        private string GetCurrentActor()
        {
            if (!_currentUser.IsAuthenticated) return "system";

            var id = _currentUser.UserId ?? "system";
            return id.Length <= 100 ? id : id[..100];
        }
    }

    public static class Extensions
    {
        public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
            entry.References.Any(r =>
            r.TargetEntry != null
            && r.TargetEntry.Metadata.IsOwned()
            && (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified)
            );
    }
}
