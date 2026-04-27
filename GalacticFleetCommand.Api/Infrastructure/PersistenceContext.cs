namespace GalacticFleetCommand.Api.Infrastructure;

public class PersistenceContext
{
    public IFleetRepository Fleets { get; } = new InMemoryFleetRepository();
    public ICommandRepository Commands { get; } = new InMemoryCommandRepository();
    public IResourcePoolRepository ResourcePools { get; } = new InMemoryResourcePoolRepository();
}