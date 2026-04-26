using GalacticFleetCommand.Api.Domain;

namespace GalacticFleetCommand.Api.Infrastructure;

public interface IRepository<T> where T : IVersionedEntity
{
    void Create(T entity);
    T? Get(string id);
    T GetOrThrow(string id);
    void Update(string id, int expectedVersion, Func<T, T> updater);
    void Delete(string id, int? expectedVersion = null);
    void Clear();
}