using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Infrastructure;

namespace GalacticFleetCommand.Api.Application;

public class CommandProcessor : ICommandProcessor
{
    private const string PrepareFleetCommandType = "PrepareFleetCommand";
    private const string FleetIdPayloadKey = "fleetId";

    private readonly ICommandRepository commandRepository;
    private readonly IFleetRepository fleetRepository;
    private readonly ILogger<CommandProcessor> logger;

    public CommandProcessor(
        ICommandRepository commandRepository,
        IFleetRepository fleetRepository,
        ILogger<CommandProcessor> logger)
    {
        this.commandRepository = commandRepository;
        this.fleetRepository = fleetRepository;
        this.logger = logger;
    }

    public Task ProcessAsync(string commandId, CancellationToken cancellationToken)
    {
        var command = commandRepository.GetOrThrow(commandId);

        MarkProcessing(command.Id);

        try
        {
            switch (command.Type)
            {
                case PrepareFleetCommandType:
                    HandlePrepareFleet(command);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown command type {command.Type}");
            }

            MarkSucceeded(command.Id);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Command {CommandId} failed", command.Id);

            MarkFailed(command.Id, ex.Message);
        }

        return Task.CompletedTask;
    }

    private void HandlePrepareFleet(Command command)
    {
        if (!command.Payload.TryGetValue(FleetIdPayloadKey, out var fleetIdValue))
            throw new InvalidOperationException("Missing fleetId");

        var fleetId = fleetIdValue?.ToString();

        if (string.IsNullOrWhiteSpace(fleetId))
            throw new InvalidOperationException("Invalid fleetId");

        var fleet = fleetRepository.GetOrThrow(fleetId);

        fleetRepository.Update(fleet.Id, fleet.Version, current =>
        {
            current.BeginPreparation();
            return current;
        });

        var preparingFleet = fleetRepository.GetOrThrow(fleet.Id);

        fleetRepository.Update(preparingFleet.Id, preparingFleet.Version, current =>
        {
            current.MarkReady();
            return current;
        });
    }

    private void MarkProcessing(string commandId)
    {
        var command = commandRepository.GetOrThrow(commandId);

        commandRepository.Update(command.Id, command.Version, current =>
        {
            current.Status = CommandStatus.Processing;
            current.FailureReason = null;
            return current;
        });
    }

    private void MarkSucceeded(string commandId)
    {
        var command = commandRepository.GetOrThrow(commandId);

        commandRepository.Update(command.Id, command.Version, current =>
        {
            current.Status = CommandStatus.Succeeded;
            current.FailureReason = null;
            return current;
        });
    }

    private void MarkFailed(string commandId, string failureReason)
    {
        var command = commandRepository.GetOrThrow(commandId);

        commandRepository.Update(command.Id, command.Version, current =>
        {
            current.Status = CommandStatus.Failed;
            current.FailureReason = failureReason;
            return current;
        });
    }
}