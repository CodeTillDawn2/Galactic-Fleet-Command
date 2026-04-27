using GalacticFleetCommand.Api.Application;
using GalacticFleetCommand.Api.Contracts.Commands;
using GalacticFleetCommand.Api.Controllers;
using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace GalacticFleetCommand.Tests.Controllers;

public class CommandControllerTests
{
    private readonly InMemoryCommandRepository commandRepository = new();
    private readonly InMemoryFleetRepository fleetRepository = new();
    private readonly InMemoryBackgroundCommandQueue queue = new();

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedCommand()
    {
        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 5,
            FuelRequired = 100
        };

        fleetRepository.Create(fleet);

        var controller = CreateController();

        var result = await controller.Create(
            new CreateCommandRequest
            {
                Type = CommandType.PrepareFleetCommand,
                FleetId = fleet.Id
            },
            CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<CommandResponse>(createdResult.Value);

        Assert.Equal(nameof(CommandController.Get), createdResult.ActionName);
        Assert.Equal(CommandType.PrepareFleetCommand, response.Type);
        Assert.Equal(CommandStatus.Queued, response.Status);
        Assert.Equal(fleet.Id, response.FleetId);
    }

    [Fact]
    public async Task Create_WithMissingFleetId_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = await controller.Create(
            new CreateCommandRequest
            {
                Type = CommandType.PrepareFleetCommand,
                FleetId = ""
            },
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_WithUnknownFleet_ReturnsNotFound()
    {
        var controller = CreateController();

        var result = await controller.Create(
            new CreateCommandRequest
            {
                Type = CommandType.PrepareFleetCommand,
                FleetId = "missing-fleet"
            },
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Get_WithExistingCommand_ReturnsOk()
    {
        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 5,
            FuelRequired = 100
        };

        fleetRepository.Create(fleet);

        var controller = CreateController();

        var createResult = Assert.IsType<CreatedAtActionResult>(
            await controller.Create(
                new CreateCommandRequest
                {
                    Type = CommandType.PrepareFleetCommand,
                    FleetId = fleet.Id
                },
                CancellationToken.None));

        var createdCommand = Assert.IsType<CommandResponse>(createResult.Value);

        var getResult = controller.Get(createdCommand.Id);

        var okResult = Assert.IsType<OkObjectResult>(getResult);
        var response = Assert.IsType<CommandResponse>(okResult.Value);

        Assert.Equal(createdCommand.Id, response.Id);
        Assert.Equal(fleet.Id, response.FleetId);
    }

    [Fact]
    public void Get_WithUnknownCommand_ReturnsNotFound()
    {
        var controller = CreateController();

        var result = controller.Get("missing-command");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_WithDeployCommand_ReturnsCreatedCommand()
    {
        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Fleet",
            ShipCount = 5,
            FuelRequired = 100
        };

        fleetRepository.Create(fleet);

        var controller = CreateController();

        var result = await controller.Create(
            new CreateCommandRequest
            {
                Type = CommandType.DeployFleetCommand,
                FleetId = fleet.Id
            },
            CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<CommandResponse>(createdResult.Value);

        Assert.Equal(nameof(CommandController.Get), createdResult.ActionName);
        Assert.Equal(CommandType.DeployFleetCommand, response.Type);
        Assert.Equal(CommandStatus.Queued, response.Status);
        Assert.Equal(fleet.Id, response.FleetId);
    }

    private CommandController CreateController()
    {
        var service = new CommandService(commandRepository, fleetRepository, queue);

        return new CommandController(
            service,
            NullLogger<CommandController>.Instance);
    }

    [Fact]
    public async Task CreateDockFleetCommand_WithValidFleet_CreatesQueuedCommand()
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
            Type = CommandType.DockFleetCommand,
            FleetId = fleet.Id
        });

        Assert.False(string.IsNullOrWhiteSpace(response.Id));
        Assert.Equal(CommandType.DockFleetCommand, response.Type);
        Assert.Equal(CommandStatus.Queued, response.Status);
        Assert.Equal(fleet.Id, response.FleetId);
    }
}