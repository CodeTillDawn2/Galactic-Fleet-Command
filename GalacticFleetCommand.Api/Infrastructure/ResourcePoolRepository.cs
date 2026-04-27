using GalacticFleetCommand.Api.Domain;

namespace GalacticFleetCommand.Api.Infrastructure;

public interface IResourcePoolRepository
{
    void Create(ResourcePool pool);
    ResourcePool? Get(string id);
    ResourcePool GetOrThrow(string id);
    ResourcePool? GetByType(ResourceType resourceType);
    void Update(string id, int expectedVersion, Func<ResourcePool, ResourcePool> updater);
    void Clear();
}

public class InMemoryResourcePoolRepository : IResourcePoolRepository
{
    private readonly Dictionary<string, ResourcePool> store = [];

    public void Create(ResourcePool pool)
    {
        if (store.ContainsKey(pool.Id))
            throw new DuplicateIdException(pool.Id);

        store[pool.Id] = pool;
    }

    public ResourcePool? Get(string id)
    {
        return store.GetValueOrDefault(id);
    }

    public ResourcePool GetOrThrow(string id)
    {
        return store.GetValueOrDefault(id) ?? throw new NotFoundException(id);
    }

    public ResourcePool? GetByType(ResourceType resourceType)
    {
        return store.Values.FirstOrDefault(p => p.ResourceType == resourceType);
    }

    public void Update(string id, int expectedVersion, Func<ResourcePool, ResourcePool> updater)
    {
        if (!store.TryGetValue(id, out var current))
            throw new NotFoundException(id);

        if (current.Version != expectedVersion)
            throw new ConcurrencyException(id, expectedVersion, current.Version);

        var updated = updater(current);
        updated.Version = expectedVersion + 1;

        store[id] = updated;
    }

    public void Clear()
    {
        store.Clear();
    }
}