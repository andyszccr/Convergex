using System.Text.Json;
using Convergex.Application.Interfaces;
using Convergex.Domain.Entities;
using Convergex.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Convergex.Persistence.Auditing;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly Dictionary<Type, string> AuditedEntities = new()
    {
        [typeof(Currency)] = "Moneda",
        [typeof(Unit)] = "Unidad",
        [typeof(ExchangeRate)] = "TasaDeCambio",
        [typeof(User)] = "Usuario",
        [typeof(Role)] = "Rol"
    };

    private static readonly Dictionary<Type, string[]> SensitiveProperties = new()
    {
        [typeof(User)] = ["PasswordHash", "ResetToken"]
    };

    private static readonly Dictionary<Type, string[]> NoiseOnlyProperties = new()
    {
        [typeof(User)] = ["LastLoginAt"]
    };

    private readonly ICurrentUserContext _currentUserContext;
    private readonly ILogger<AuditSaveChangesInterceptor> _logger;
    private List<PendingAuditEntry> _pending = [];

    public AuditSaveChangesInterceptor(ICurrentUserContext currentUserContext, ILogger<AuditSaveChangesInterceptor> logger)
    {
        _currentUserContext = currentUserContext;
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _pending = BuildPendingEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var pending = _pending;
        _pending = [];

        if (pending.Count > 0 && eventData.Context is not null)
        {
            await PersistAuditLogsAsync(eventData.Context, pending, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private List<PendingAuditEntry> BuildPendingEntries(DbContext? context)
    {
        if (context is null)
        {
            return [];
        }

        try
        {
            var pending = new List<PendingAuditEntry>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                {
                    continue;
                }

                var type = entry.Entity.GetType();
                if (!AuditedEntities.TryGetValue(type, out var entityName))
                {
                    continue;
                }

                if (entry.State == EntityState.Modified && IsNoiseOnlyChange(type, entry))
                {
                    continue;
                }

                pending.Add(new PendingAuditEntry
                {
                    Entity = entry.Entity,
                    EntityName = entityName,
                    Action = entry.State switch
                    {
                        EntityState.Added => AuditAction.Create,
                        EntityState.Deleted => AuditAction.Delete,
                        _ => AuditAction.Update
                    },
                    OldValues = entry.State == EntityState.Added ? null : SerializeValues(type, entry.OriginalValues),
                    NewValues = entry.State == EntityState.Deleted ? null : SerializeValues(type, entry.CurrentValues)
                });
            }

            return pending;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron preparar los registros de auditoría antes de guardar cambios.");
            return [];
        }
    }

    private async Task PersistAuditLogsAsync(DbContext context, List<PendingAuditEntry> pending, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserContext.UserId;
            var userName = _currentUserContext.UserName ?? "Sistema";
            var ipAddress = _currentUserContext.IpAddress;

            var logs = pending.Select(p => new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = p.Action,
                EntityName = p.EntityName,
                EntityId = GetEntityId(p.Entity),
                OldValues = p.OldValues,
                NewValues = p.NewValues,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                Status = AuditStatus.Success
            }).ToList();

            context.Set<AuditLog>().AddRange(logs);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron persistir los registros de auditoría generados automáticamente.");
        }
    }

    private static bool IsNoiseOnlyChange(Type type, EntityEntry entry)
    {
        if (!NoiseOnlyProperties.TryGetValue(type, out var noiseProperties))
        {
            return false;
        }

        // DbSet.Update() marca todas las propiedades como IsModified aunque su valor no haya cambiado,
        // por lo que se compara Original vs Current para detectar cambios reales (ej. solo LastLoginAt en cada login).
        var changedProperties = entry.Properties
            .Where(p => !Equals(p.OriginalValue, p.CurrentValue))
            .Select(p => p.Metadata.Name)
            .ToList();

        return changedProperties.Count > 0 && changedProperties.All(noiseProperties.Contains);
    }

    private static string SerializeValues(Type type, PropertyValues values)
    {
        var sensitive = SensitiveProperties.TryGetValue(type, out var props) ? props : [];
        var dict = new Dictionary<string, object?>();

        foreach (var property in values.Properties)
        {
            // "Id" se omite: para entidades nuevas aún no existe el valor definitivo generado por la base de
            // datos en este punto (previo al guardado); el identificador real ya se expone en AuditLog.EntityId.
            if (property.Name == "Id")
            {
                continue;
            }

            dict[property.Name] = sensitive.Contains(property.Name) ? "***" : values[property.Name];
        }

        return JsonSerializer.Serialize(dict);
    }

    private static string? GetEntityId(object entity)
        => entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString();

    private class PendingAuditEntry
    {
        public object Entity { get; set; } = null!;
        public string EntityName { get; set; } = string.Empty;
        public AuditAction Action { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }
}
