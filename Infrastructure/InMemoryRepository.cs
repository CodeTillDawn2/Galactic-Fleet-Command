using GalacticFleetCommand.Api.Domain;

namespace GalacticFleetCommand.Api.Infrastructure;

public class InMemoryRepository<T> : IRepository<T> where T : IVersionedEntity
{
    private readonly Dictionary<string, T> store = [];

    public void Create(T entity)
    {
        if (store.ContainsKey(entity.Id))
            throw new DuplicateIdException(entity.Id);

        store[entity.Id] = entity;
    }

    public T? Get(string id)
    {
        return store.GetValueOrDefault(id);
    }

    public T GetOrThrow(string id)
    {
        return store.GetValueOrDefault(id) ?? throw new NotFoundException(id);
    }

    public void Update(string id, int expectedVersion, Func<T, T> updater)
    {
        if (!store.TryGetValue(id, out var current))
            throw new NotFoundException(id);

        if (current.Version != expectedVersion)
            throw new ConcurrencyException(id, expectedVersion, current.Version);

        var updated = updater(current);
        updated.Version = expectedVersion + 1;

        store[id] = updated;
    }

    public void Delete(string id, int? expectedVersion = null)
    {
        if (!store.TryGetValue(id, out var current))
            throw new NotFoundException(id);

        if (expectedVersion is not null && current.Version != expectedVersion.Value)
            throw new ConcurrencyException(id, expectedVersion.Value, current.Version);

        store.Remove(id);
    }

    public void Clear()
    {
        store.Clear();
    }
}