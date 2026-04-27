using GalacticFleetCommand.Api.Domain;

namespace GalacticFleetCommand.Api.Infrastructure;
public class PersistenceContext
{
    public PersistenceContext()
    {
        ResourcePools.Create(new ResourcePool
        {
            Id = "fuel-pool",
            ResourceType = ResourceType.Fuel,
            Total = 100,
            Reserved = 0
        });
    }

    public IFleetRepository Fleets { get; } = new InMemoryFleetRepository();
    public ICommandRepository Commands { get; } = new InMemoryCommandRepository();
    public IResourcePoolRepository ResourcePools { get; } = new InMemoryResourcePoolRepository();
}