using GalacticFleetCommand.Api.Domain;

namespace GalacticFleetCommand.Api.Infrastructure;

public interface ICommandRepository : IRepository<Command>;

public class InMemoryCommandRepository : InMemoryRepository<Command>, ICommandRepository;