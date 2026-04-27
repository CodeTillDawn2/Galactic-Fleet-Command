using GalacticFleetCommand.Api.Infrastructure;

namespace GalacticFleetCommand.Api.Application;

public class CommandProcessor : ICommandProcessor
{
    private readonly ICommandRepository commandRepository;
    private readonly ILogger<CommandProcessor> logger;

    public CommandProcessor(ICommandRepository commandRepository, ILogger<CommandProcessor> logger)
    {
        this.commandRepository = commandRepository;
        this.logger = logger;
    }

    public Task ProcessAsync(string commandId, CancellationToken cancellationToken)
    {
        var command = commandRepository.GetOrThrow(commandId);

        logger.LogInformation(
            "Dequeued command {CommandId} of type {CommandType}",
            command.Id,
            command.Type);

        return Task.CompletedTask;
    }
}