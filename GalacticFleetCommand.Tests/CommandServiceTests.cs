using GalacticFleetCommand.Api.Application;
using GalacticFleetCommand.Api.Contracts.Commands;
using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Infrastructure;

namespace GalacticFleetCommand.Tests.Application;

public class CommandServiceTests
{
    private readonly InMemoryCommandRepository commandRepository = new();
    private readonly InMemoryFleetRepository fleetRepository = new();
    private readonly InMemoryBackgroundCommandQueue queue = new();

    [Fact]
    public async Task CreatePrepareFleetCommand_WithValidFleet_CreatesQueuedCommand()
    {
        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 5,
            FuelRequired = 100
        };

        fleetRepository.Create(fleet);

        var service = new CommandService(commandRepository, fleetRepository, queue);

        var response = await service.CreateCommandAsync(new CreateCommandRequest
        {
            Type = CommandType.PrepareFleetCommand,
            FleetId = fleet.Id
        });

        Assert.False(string.IsNullOrWhiteSpace(response.Id));
        Assert.Equal(CommandType.PrepareFleetCommand, response.Type);
        Assert.Equal(CommandStatus.Queued, response.Status);
        Assert.Equal(fleet.Id, response.FleetId);
    }

    [Fact]
    public async Task CreateDeployFleetCommand_WithValidFleet_CreatesQueuedCommand()
    {
        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 5,
            FuelRequired = 100
        };

        fleetRepository.Create(fleet);

        var service = new CommandService(commandRepository, fleetRepository, queue);

        var response = await service.CreateCommandAsync(new CreateCommandRequest
        {
            Type = CommandType.DeployFleetCommand,
            FleetId = fleet.Id
        });

        Assert.False(string.IsNullOrWhiteSpace(response.Id));
        Assert.Equal(CommandType.DeployFleetCommand, response.Type);
        Assert.Equal(CommandStatus.Queued, response.Status);
        Assert.Equal(fleet.Id, response.FleetId);
    }

    [Fact]
    public async Task CreatePrepareFleetCommand_WithMissingFleetId_ThrowsArgumentException()
    {
        var service = new CommandService(commandRepository, fleetRepository, queue);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateCommandAsync(new CreateCommandRequest
            {
                Type = CommandType.PrepareFleetCommand,
                FleetId = ""
            }));

        Assert.Equal("Fleet id is required", exception.Message);
    }

    [Fact]
    public async Task CreatePrepareFleetCommand_WithUnknownFleet_ThrowsNotFoundException()
    {
        var service = new CommandService(commandRepository, fleetRepository, queue);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateCommandAsync(new CreateCommandRequest
            {
                Type = CommandType.PrepareFleetCommand,
                FleetId = "missing-fleet"
            }));
    }

    [Fact]
    public async Task Get_WithExistingCommand_ReturnsCommand()
    {
        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 5,
            FuelRequired = 100
        };

        fleetRepository.Create(fleet);

        var service = new CommandService(commandRepository, fleetRepository, queue);

        var created = await service.CreateCommandAsync(new CreateCommandRequest
        {
            Type = CommandType.PrepareFleetCommand,
            FleetId = fleet.Id
        });

        var fetched = service.Get(created.Id);

        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Type, fetched.Type);
        Assert.Equal(created.Status, fetched.Status);
        Assert.Equal(created.FleetId, fetched.FleetId);
    }

    [Fact]
    public void Get_WithUnknownCommand_ThrowsNotFoundException()
    {
        var service = new CommandService(commandRepository, fleetRepository, queue);

        Assert.Throws<NotFoundException>(() => service.Get("missing-command"));
    }
}