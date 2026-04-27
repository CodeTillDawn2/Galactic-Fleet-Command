using GalacticFleetCommand.Api.Application;
using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace GalacticFleetCommand.Tests.Application;

public class PrepareFleetCommandTests
{
    [Fact]
    public async Task ProcessAsync_PrepareFleetCommand_TransitionsFleetToReady()
    {
        var fleetRepo = new InMemoryFleetRepository();
        var commandRepo = new InMemoryCommandRepository();

        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            ShipCount = 1,
            FuelRequired = 10
        };

        fleetRepo.Create(fleet);

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

        commandRepo.Create(command);

        var processor = new CommandProcessor(
            commandRepo,
            fleetRepo,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);

        var updatedFleet = fleetRepo.GetOrThrow(fleet.Id);
        var updatedCommand = commandRepo.GetOrThrow(command.Id);

        Assert.Equal(FleetState.Ready, updatedFleet.State);
        Assert.Equal(CommandStatus.Succeeded, updatedCommand.Status);
    }

    [Fact]
    public async Task ProcessAsync_InvalidState_RecordsFailure()
    {
        var fleetRepo = new InMemoryFleetRepository();
        var commandRepo = new InMemoryCommandRepository();

        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test",
            ShipCount = 1,
            FuelRequired = 10
        };

        fleet.BeginPreparation(); // invalid starting state

        fleetRepo.Create(fleet);

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

        commandRepo.Create(command);

        var processor = new CommandProcessor(
            commandRepo,
            fleetRepo,
            NullLogger<CommandProcessor>.Instance);

        await processor.ProcessAsync(command.Id, CancellationToken.None);

        var updatedCommand = commandRepo.GetOrThrow(command.Id);

        Assert.Equal(CommandStatus.Failed, updatedCommand.Status);
        Assert.False(string.IsNullOrWhiteSpace(updatedCommand.FailureReason));
    }
}