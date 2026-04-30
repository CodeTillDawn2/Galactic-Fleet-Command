using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Infrastructure;

namespace GalacticFleetCommand.Api.Application;

public class CommandProcessor : ICommandProcessor
{
    private const string FleetIdPayloadKey = "fleetId";

    private readonly ICommandRepository commandRepository;
    private readonly IFleetRepository fleetRepository;
    private readonly IResourcePoolRepository resourcePoolRepository;
    private readonly ILogger<CommandProcessor> logger;

    public CommandProcessor(
        ICommandRepository commandRepository,
        IFleetRepository fleetRepository,
        IResourcePoolRepository resourcePoolRepository,
        ILogger<CommandProcessor> logger)
    {
        this.commandRepository = commandRepository;
        this.fleetRepository = fleetRepository;
        this.resourcePoolRepository = resourcePoolRepository;
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
                case CommandType.PrepareFleetCommand:
                    HandlePrepareFleet(command);
                    break;
                case CommandType.DeployFleetCommand:
                    HandleDeployFleet(command);
                    break;
                case CommandType.DockFleetCommand:
                    HandleDockFleet(command);
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

        var preparationStarted = false;

        fleetRepository.Update(fleet.Id, fleet.Version, current =>
        {
            current.BeginPreparation();
            preparationStarted = true;
            return current;
        });

        try
        {
            var preparingFleet = fleetRepository.GetOrThrow(fleet.Id);

            var pool = resourcePoolRepository.GetByType(ResourceType.Fuel)
                ?? throw new InvalidOperationException("Fuel pool not configured");

            var reservationSucceeded = false;

            resourcePoolRepository.Update(pool.Id, pool.Version, current =>
            {
                var available = current.Total - current.Reserved;

                if (available >= preparingFleet.FuelRequired)
                {
                    current.Reserved += preparingFleet.FuelRequired;
                    reservationSucceeded = true;
                }

                return current;
            });

            var updatedFleet = fleetRepository.GetOrThrow(preparingFleet.Id);

            fleetRepository.Update(updatedFleet.Id, updatedFleet.Version, current =>
            {
                if (reservationSucceeded)
                    current.MarkReady();
                else
                    current.FailPreparation();

                return current;
            });
        }
        catch
        {
            if (preparationStarted)
            {
                var currentFleet = fleetRepository.GetOrThrow(fleet.Id);

                if (currentFleet.State == FleetState.Preparing)
                {
                    fleetRepository.Update(currentFleet.Id, currentFleet.Version, current =>
                    {
                        current.FailPreparation();
                        return current;
                    });
                }
            }

            throw;
        }
    }

    private void HandleDeployFleet(Command command)
    {
        if (!command.Payload.TryGetValue(FleetIdPayloadKey, out var fleetIdValue))
            throw new InvalidOperationException("Missing fleetId");

        var fleetId = fleetIdValue?.ToString();

        if (string.IsNullOrWhiteSpace(fleetId))
            throw new InvalidOperationException("Invalid fleetId");

        var fleet = fleetRepository.GetOrThrow(fleetId);

        fleetRepository.Update(fleet.Id, fleet.Version, current =>
        {
            current.Deploy();
            return current;
        });
    }

    private void HandleDockFleet(Command command)
    {
        var fleetId = GetFleetId(command);
        var fleet = fleetRepository.GetOrThrow(fleetId);

        fleetRepository.Update(fleet.Id, fleet.Version, current =>
        {
            current.Dock();
            return current;
        });

        var pool = resourcePoolRepository.GetByType(ResourceType.Fuel)
            ?? throw new InvalidOperationException("Fuel pool not configured");

        resourcePoolRepository.Update(pool.Id, pool.Version, current =>
        {
            current.Reserved = Math.Max(0, current.Reserved - fleet.FuelRequired);
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

    private static string GetFleetId(Command command)
    {
        if (!command.Payload.TryGetValue(FleetIdPayloadKey, out var fleetIdValue))
            throw new InvalidOperationException("Missing fleetId");

        var fleetId = fleetIdValue?.ToString();

        if (string.IsNullOrWhiteSpace(fleetId))
            throw new InvalidOperationException("Invalid fleetId");

        return fleetId;
    }
}