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
        var resourcePoolRepository = new InMemoryResourcePoolRepository();

        var resourcePool = new ResourcePool
        {
            Id = Guid.NewGuid().ToString(),
            ResourceType = ResourceType.Fuel,
            Total = 100,
            Reserved = 0
        };

        resourcePoolRepository.Create(resourcePool);

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
            Type = CommandType.PrepareFleetCommand,
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
            resourcePoolRepository,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);
    }

    [Fact]
    public async Task ProcessAsync_WithUnknownCommand_ThrowsNotFoundException()
    {
        var processor = new CommandProcessor(
            new InMemoryCommandRepository(),
            new InMemoryFleetRepository(),
            new InMemoryResourcePoolRepository(),
            NullLogger<CommandProcessor>.Instance);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            processor.ProcessAsync("missing-command", CancellationToken.None));
    }

    [Fact]
    public async Task ProcessAsync_PrepareFleetCommand_TransitionsFleetToReady()
    {
        var commandRepository = new InMemoryCommandRepository();
        var fleetRepository = new InMemoryFleetRepository();
        var resourcePoolRepository = new InMemoryResourcePoolRepository();

        var resourcePool = new ResourcePool
        {
            Id = Guid.NewGuid().ToString(),
            ResourceType = ResourceType.Fuel,
            Total = 100,
            Reserved = 0
        };

        resourcePoolRepository.Create(resourcePool);

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
            Type = CommandType.PrepareFleetCommand,
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
            resourcePoolRepository,
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
        var resourcePoolRepository = new InMemoryResourcePoolRepository();

        var resourcePool = new ResourcePool
        {
            Id = Guid.NewGuid().ToString(),
            ResourceType = ResourceType.Fuel,
            Total = 100,
            Reserved = 0
        };

        resourcePoolRepository.Create(resourcePool);

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
            Type = CommandType.PrepareFleetCommand,
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
            resourcePoolRepository,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);

        var updatedCommand = commandRepository.GetOrThrow(command.Id);

        Assert.Equal(CommandStatus.Failed, updatedCommand.Status);
        Assert.False(string.IsNullOrWhiteSpace(updatedCommand.FailureReason));
    }

    /// <summary>
    /// Verifies that processing a PrepareFleetCommand results in a complete and consistent lifecycle:
    /// - Command transitions from Queued → Processing → Succeeded
    /// - Fleet transitions from Docked → Preparing → Ready
    /// - Versioned entities are updated correctly across multiple repository updates
    /// - No failure state or data corruption occurs during the process
    /// </summary>
    [Fact]
    public async Task ProcessAsync_PrepareFleetCommand_CompletesLifecycleSuccessfully()
    {
        var commandRepository = new InMemoryCommandRepository();
        var fleetRepository = new InMemoryFleetRepository();
        var resourcePoolRepository = new InMemoryResourcePoolRepository();

        var resourcePool = new ResourcePool
        {
            Id = Guid.NewGuid().ToString(),
            ResourceType = ResourceType.Fuel,
            Total = 100,
            Reserved = 0
        };

        resourcePoolRepository.Create(resourcePool);

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
            Type = CommandType.PrepareFleetCommand,
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
            resourcePoolRepository,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);

        var updatedCommand = commandRepository.GetOrThrow(command.Id);
        var updatedFleet = fleetRepository.GetOrThrow(fleet.Id);
        var updatedResourcePool = resourcePoolRepository.GetOrThrow(resourcePool.Id);

        Assert.Equal(CommandStatus.Succeeded, updatedCommand.Status);
        Assert.Null(updatedCommand.FailureReason);
        Assert.Equal(2, updatedCommand.Version);

        Assert.Equal(FleetState.Ready, updatedFleet.State);
        Assert.Equal(2, updatedFleet.Version);

        Assert.Equal(10, updatedResourcePool.Reserved);
        Assert.Equal(resourcePool.Total, updatedResourcePool.Total);
        Assert.Equal(fleet.Id, updatedCommand.Payload["fleetId"]);
    }

    [Fact]
    public async Task ProcessAsync_PrepareFleetCommand_WithSufficientFuel_ReservesFuelAndMarksReady()
    {
        var commandRepository = new InMemoryCommandRepository();
        var fleetRepository = new InMemoryFleetRepository();
        var resourcePoolRepository = new InMemoryResourcePoolRepository();

        var resourcePool = new ResourcePool
        {
            Id = Guid.NewGuid().ToString(),
            ResourceType = ResourceType.Fuel,
            Total = 100,
            Reserved = 0
        };

        resourcePoolRepository.Create(resourcePool);

        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 1,
            FuelRequired = 50
        };

        fleetRepository.Create(fleet);

        var command = new Command
        {
            Id = Guid.NewGuid().ToString(),
            Type = CommandType.PrepareFleetCommand,
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
            resourcePoolRepository,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);

        var updatedFleet = fleetRepository.GetOrThrow(fleet.Id);
        var updatedCommand = commandRepository.GetOrThrow(command.Id);
        var updatedResourcePool = resourcePoolRepository.GetOrThrow(resourcePool.Id);

        Assert.Equal(FleetState.Ready, updatedFleet.State);
        Assert.Equal(CommandStatus.Succeeded, updatedCommand.Status);
        Assert.Equal(50, updatedResourcePool.Reserved);
    }

    [Fact]
    public async Task ProcessAsync_PrepareFleetCommand_WithInsufficientFuel_FailsPreparation()
    {
        var commandRepository = new InMemoryCommandRepository();
        var fleetRepository = new InMemoryFleetRepository();
        var resourcePoolRepository = new InMemoryResourcePoolRepository();

        var resourcePool = new ResourcePool
        {
            Id = Guid.NewGuid().ToString(),
            ResourceType = ResourceType.Fuel,
            Total = 10,
            Reserved = 0
        };

        resourcePoolRepository.Create(resourcePool);

        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 1,
            FuelRequired = 50
        };

        fleetRepository.Create(fleet);

        var command = new Command
        {
            Id = Guid.NewGuid().ToString(),
            Type = CommandType.PrepareFleetCommand,
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
            resourcePoolRepository,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);

        var updatedFleet = fleetRepository.GetOrThrow(fleet.Id);
        var updatedCommand = commandRepository.GetOrThrow(command.Id);
        var updatedResourcePool = resourcePoolRepository.GetOrThrow(resourcePool.Id);

        Assert.Equal(FleetState.FailedPreparation, updatedFleet.State);
        Assert.Equal(CommandStatus.Succeeded, updatedCommand.Status);
        Assert.Equal(0, updatedResourcePool.Reserved);
    }

    /// <summary>
    /// Simulates many commands attempting to reserve fuel concurrently against a shared resource pool.
    /// Uses a synchronized start to increase contention and verify that reservation logic remains atomic:
    /// - Total reserved fuel never exceeds available capacity
    /// - Only the number of fleets supported by available fuel become ready
    /// - Remaining fleets fail preparation without corrupting shared state
    /// </summary>
    [Fact]
    public async Task ProcessAsync_WithConcurrentCommands_DoesNotOverAllocateFuel()
    {
        var commandRepository = new InMemoryCommandRepository();
        var fleetRepository = new InMemoryFleetRepository();
        var resourcePoolRepository = new InMemoryResourcePoolRepository();

        const int commandCount = 50;
        const int totalFuel = 100;
        const int fuelRequiredPerFleet = 30;
        const int expectedReadyFleetCount = 3;

        var resourcePool = new ResourcePool
        {
            Id = Guid.NewGuid().ToString(),
            ResourceType = ResourceType.Fuel,
            Total = totalFuel,
            Reserved = 0
        };

        resourcePoolRepository.Create(resourcePool);

        var commandIds = new List<string>();

        for (var i = 0; i < commandCount; i++)
        {
            var fleet = new Fleet
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Fleet {i}",
                ShipCount = 1,
                FuelRequired = fuelRequiredPerFleet
            };

            fleetRepository.Create(fleet);

            var command = new Command
            {
                Id = Guid.NewGuid().ToString(),
                Type = CommandType.PrepareFleetCommand,
                Status = CommandStatus.Queued,
                Payload = new Dictionary<string, object?>
                {
                    ["fleetId"] = fleet.Id
                }
            };

            commandRepository.Create(command);
            commandIds.Add(command.Id);
        }

        var processor = new CommandProcessor(
            commandRepository,
            fleetRepository,
            resourcePoolRepository,
            NullLogger<CommandProcessor>.Instance);

        var startGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var tasks = commandIds.Select(async commandId =>
        {
            await startGate.Task;
            await processor.ProcessAsync(commandId, CancellationToken.None);
        });

        startGate.SetResult();

        await Task.WhenAll(tasks);

        var updatedResourcePool = resourcePoolRepository.GetOrThrow(resourcePool.Id);

        var fleets = commandIds
            .Select(commandRepository.GetOrThrow)
            .Select(command => command.Payload["fleetId"]?.ToString())
            .Where(fleetId => !string.IsNullOrWhiteSpace(fleetId))
            .Select(fleetId => fleetRepository.GetOrThrow(fleetId!))
            .ToList();

        var readyFleetCount = fleets.Count(fleet => fleet.State == FleetState.Ready);
        var failedFleetCount = fleets.Count(fleet => fleet.State == FleetState.FailedPreparation);

        Assert.True(updatedResourcePool.Reserved <= updatedResourcePool.Total);
        Assert.Equal(expectedReadyFleetCount * fuelRequiredPerFleet, updatedResourcePool.Reserved);
        Assert.Equal(expectedReadyFleetCount, readyFleetCount);
        Assert.Equal(commandCount - expectedReadyFleetCount, failedFleetCount);
    }

    [Fact]
    public async Task ProcessAsync_DeployFleetCommand_TransitionsFleetToDeployed()
    {
        var commandRepo = new InMemoryCommandRepository();
        var fleetRepo = new InMemoryFleetRepository();
        var resourceRepo = new InMemoryResourcePoolRepository();

        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            ShipCount = 1,
            FuelRequired = 10
        };

        fleet.BeginPreparation();
        fleet.MarkReady();

        fleetRepo.Create(fleet);

        var command = new Command
        {
            Id = Guid.NewGuid().ToString(),
            Type = CommandType.DeployFleetCommand,
            Status = CommandStatus.Queued,
            Payload = new Dictionary<string, object?>
            {
                ["fleetId"] = fleet.Id
            }
        };

        commandRepo.Create(command);

        var processor = new CommandProcessor(
            commandRepo,
            fleetRepo,
            resourceRepo,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);

        var updatedFleet = fleetRepo.GetOrThrow(fleet.Id);
        var updatedCommand = commandRepo.GetOrThrow(command.Id);

        Assert.Equal(FleetState.Deployed, updatedFleet.State);
        Assert.Equal(CommandStatus.Succeeded, updatedCommand.Status);
        Assert.Null(updatedCommand.FailureReason);
    }

    [Fact]
    public async Task ProcessAsync_DeployFleetCommand_WhenNotReady_RecordsFailure()
    {
        var commandRepo = new InMemoryCommandRepository();
        var fleetRepo = new InMemoryFleetRepository();
        var resourceRepo = new InMemoryResourcePoolRepository();

        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            ShipCount = 1,
            FuelRequired = 10
        };

        fleetRepo.Create(fleet); // still Docked

        var command = new Command
        {
            Id = Guid.NewGuid().ToString(),
            Type = CommandType.DeployFleetCommand,
            Status = CommandStatus.Queued,
            Payload = new Dictionary<string, object?>
            {
                ["fleetId"] = fleet.Id
            }
        };

        commandRepo.Create(command);

        var processor = new CommandProcessor(
            commandRepo,
            fleetRepo,
            resourceRepo,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);

        var updatedCommand = commandRepo.GetOrThrow(command.Id);

        Assert.Equal(CommandStatus.Failed, updatedCommand.Status);
    }

    [Fact]
    public async Task ProcessAsync_PrepareFleetCommand_RecordsTransitionHistory()
    {
        var commandRepository = new InMemoryCommandRepository();
        var fleetRepository = new InMemoryFleetRepository();
        var resourcePoolRepository = new InMemoryResourcePoolRepository();

        var resourcePool = new ResourcePool
        {
            Id = Guid.NewGuid().ToString(),
            ResourceType = ResourceType.Fuel,
            Total = 100,
            Reserved = 0
        };

        resourcePoolRepository.Create(resourcePool);

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
            Type = CommandType.PrepareFleetCommand,
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
            resourcePoolRepository,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);

        var updatedFleet = fleetRepository.GetOrThrow(fleet.Id);

        Assert.Equal(2, updatedFleet.Transitions.Count);

        Assert.Equal(FleetState.Docked, updatedFleet.Transitions[0].From);
        Assert.Equal(FleetState.Preparing, updatedFleet.Transitions[0].To);

        Assert.Equal(FleetState.Preparing, updatedFleet.Transitions[1].From);
        Assert.Equal(FleetState.Ready, updatedFleet.Transitions[1].To);
    }
}