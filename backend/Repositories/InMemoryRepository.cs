using System.Collections.Concurrent;
using AutoGenBackend.Models;

namespace AutoGenBackend.Repositories;

/// <summary>
/// In-memory repository implementation for development and testing
/// </summary>
public abstract class InMemoryRepository<T> : IRepository<T> where T : class
{
    protected readonly ConcurrentDictionary<Guid, T> _storage = new();

    public virtual Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _storage.TryGetValue(id, out var entity);
        return Task.FromResult(entity);
    }

    public virtual Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.Values.AsEnumerable());
    }

    public virtual Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty != null && idProperty.PropertyType == typeof(Guid))
        {
            var currentValue = (Guid?)idProperty.GetValue(entity);
            if (currentValue == null || currentValue == Guid.Empty)
            {
                idProperty.SetValue(entity, Guid.NewGuid());
            }
        }

        var id = (Guid?)idProperty?.GetValue(entity) ?? Guid.NewGuid();
        _storage.TryAdd(id, entity);
        return Task.FromResult(entity);
    }

    public virtual Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var idProperty = typeof(T).GetProperty("Id");
        var id = (Guid?)idProperty?.GetValue(entity) ?? Guid.NewGuid();
        _storage.AddOrUpdate(id, entity, (_, _) => entity);
        return Task.FromResult(entity);
    }

    public virtual Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _storage.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public virtual Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.ContainsKey(id));
    }
}
