using GalacticFleetCommand.Api.Domain;

namespace GalacticFleetCommand.Api.Infrastructure;

public interface IFleetRepository : IRepository<Fleet>;

public class InMemoryFleetRepository : InMemoryRepository<Fleet>, IFleetRepository;