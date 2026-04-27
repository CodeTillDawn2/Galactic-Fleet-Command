using GalacticFleetCommand.Api.Contracts.Commands;
using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Infrastructure;

namespace GalacticFleetCommand.Api.Application;

public class CommandService
{
    private const string PrepareFleetCommandType = "PrepareFleetCommand";
    private const string FleetIdPayloadKey = "fleetId";

    private readonly ICommandRepository commandRepository;
    private readonly IFleetRepository fleetRepository;

    public CommandService(ICommandRepository commandRepository, IFleetRepository fleetRepository)
    {
        this.commandRepository = commandRepository;
        this.fleetRepository = fleetRepository;
    }

    public CommandResponse CreatePrepareFleetCommand(CreatePrepareFleetCommandRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FleetId))
            throw new ArgumentException("Fleet id is required");

        fleetRepository.GetOrThrow(request.FleetId);

        var command = new Command
        {
            Id = Guid.NewGuid().ToString(),
            Type = PrepareFleetCommandType,
            Status = CommandStatus.Queued,
            Payload = new Dictionary<string, object?>
            {
                [FleetIdPayloadKey] = request.FleetId
            }
        };

        commandRepository.Create(command);

        return Map(command);
    }

    public CommandResponse Get(string id)
    {
        var command = commandRepository.GetOrThrow(id);
        return Map(command);
    }

    private static CommandResponse Map(Command command)
    {
        if (!command.Payload.TryGetValue(FleetIdPayloadKey, out var fleetIdValue))
            throw new InvalidOperationException($"Command {command.Id} is missing required payload value '{FleetIdPayloadKey}'");

        var fleetId = fleetIdValue?.ToString();

        if (string.IsNullOrWhiteSpace(fleetId))
            throw new InvalidOperationException($"Command {command.Id} has an invalid fleet id payload value");

        return new CommandResponse
        {
            Id = command.Id,
            Type = command.Type,
            Status = command.Status,
            FleetId = fleetId
        };
    }
}