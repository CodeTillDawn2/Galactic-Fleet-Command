using GalacticFleetCommand.Api.Application;
using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace GalacticFleetCommand.Tests.Application;

public class CommandProcessorTests
{
    [Fact]
    public async Task ProcessAsync_WithExistingCommand_Completes()
    {
        var commandRepository = new InMemoryCommandRepository();
        var fleetRepository = new InMemoryFleetRepository();

        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 1,
            FuelRequired = 10
        };

        fleetRepository.Create(fleet);

        var command = new Command
        {
            Id = Guid.NewGuid().ToString(),
            Type = "PrepareFleetCommand",
            Status = CommandStatus.Queued,
            Payload = new Dictionary<string, object?>
            {
                ["fleetId"] = fleet.Id
            }
        };

        commandRepository.Create(command);

        var processor = new CommandProcessor(
            commandRepository,
            fleetRepository,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);
    }

    [Fact]
    public async Task ProcessAsync_WithUnknownCommand_ThrowsNotFoundException()
    {
        var processor = new CommandProcessor(
            new InMemoryCommandRepository(),
            new InMemoryFleetRepository(),
            NullLogger<CommandProcessor>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            processor.ProcessAsync("missing-command", CancellationToken.None));
    }

    [Fact]
    public async Task ProcessAsync_PrepareFleetCommand_TransitionsFleetToReady()
    {
        var commandRepository = new InMemoryCommandRepository();
        var fleetRepository = new InMemoryFleetRepository();

        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 1,
            FuelRequired = 10
        };

        fleetRepository.Create(fleet);

        var command = new Command
        {
            Id = Guid.NewGuid().ToString(),
            Type = "PrepareFleetCommand",
            Status = CommandStatus.Queued,
            Payload = new Dictionary<string, object?>
            {
                ["fleetId"] = fleet.Id
            }
        };

        commandRepository.Create(command);

        var processor = new CommandProcessor(
            commandRepository,
            fleetRepository,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);

        var updatedFleet = fleetRepository.GetOrThrow(fleet.Id);
        var updatedCommand = commandRepository.GetOrThrow(command.Id);

        Assert.Equal(FleetState.Ready, updatedFleet.State);
        Assert.Equal(CommandStatus.Succeeded, updatedCommand.Status);
    }

    [Fact]
    public async Task ProcessAsync_PrepareFleetCommand_WhenFleetIsNotDocked_RecordsFailure()
    {
        var commandRepository = new InMemoryCommandRepository();
        var fleetRepository = new InMemoryFleetRepository();

        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 1,
            FuelRequired = 10
        };

        fleet.BeginPreparation();

        fleetRepository.Create(fleet);

        var command = new Command
        {
            Id = Guid.NewGuid().ToString(),
            Type = "PrepareFleetCommand",
            Status = CommandStatus.Queued,
            Payload = new Dictionary<string, object?>
            {
                ["fleetId"] = fleet.Id
            }
        };

        commandRepository.Create(command);

        var processor = new CommandProcessor(
            commandRepository,
            fleetRepository,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);

        var updatedCommand = commandRepository.GetOrThrow(command.Id);

        Assert.Equal(CommandStatus.Failed, updatedCommand.Status);
        Assert.False(string.IsNullOrWhiteSpace(updatedCommand.FailureReason));
    }

    /// <summary>
    /// Verifies that processing a PrepareFleetCommand results in a complete and consistent lifecycle:
    /// - Command transitions from Queued -> Processing -> Succeeded
    /// - Fleet transitions from Docked -> Preparing -> Ready
    /// - Versioned entities are updated correctly across multiple repository updates
    /// - No failure state or data corruption occurs during the process
    /// </summary>
    [Fact]
    public async Task ProcessAsync_PrepareFleetCommand_CompletesLifecycleSuccessfully()
    {
        var commandRepository = new InMemoryCommandRepository();
        var fleetRepository = new InMemoryFleetRepository();

        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 1,
            FuelRequired = 10
        };

        fleetRepository.Create(fleet);

        var command = new Command
        {
            Id = Guid.NewGuid().ToString(),
            Type = "PrepareFleetCommand",
            Status = CommandStatus.Queued,
            Payload = new Dictionary<string, object?>
            {
                ["fleetId"] = fleet.Id
            }
        };

        commandRepository.Create(command);

        Assert.Equal(0, commandRepository.GetOrThrow(command.Id).Version);
        Assert.Equal(CommandStatus.Queued, commandRepository.GetOrThrow(command.Id).Status);
        Assert.Equal(0, fleetRepository.GetOrThrow(fleet.Id).Version);
        Assert.Equal(FleetState.Docked, fleetRepository.GetOrThrow(fleet.Id).State);

        var processor = new CommandProcessor(
            commandRepository,
            fleetRepository,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);

        var updatedCommand = commandRepository.GetOrThrow(command.Id);
        var updatedFleet = fleetRepository.GetOrThrow(fleet.Id);

        Assert.Equal(CommandStatus.Succeeded, updatedCommand.Status);
        Assert.Null(updatedCommand.FailureReason);
        Assert.Equal(2, updatedCommand.Version);

        Assert.Equal(FleetState.Ready, updatedFleet.State);
        Assert.Equal(2, updatedFleet.Version);

        Assert.Equal(fleet.Id, updatedCommand.Payload["fleetId"]);
    }
}