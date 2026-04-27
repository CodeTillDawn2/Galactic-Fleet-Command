using GalacticFleetCommand.Api.Contracts.Commands;
using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Infrastructure;

namespace GalacticFleetCommand.Api.Application;

public class CommandService
{
    private const string FleetIdPayloadKey = "fleetId";

    private readonly ICommandRepository commandRepository;
    private readonly IFleetRepository fleetRepository;
    private readonly IBackgroundCommandQueue queue;

    public CommandService(
        ICommandRepository commandRepository,
        IFleetRepository fleetRepository,
        IBackgroundCommandQueue queue)
    {
        this.commandRepository = commandRepository;
        this.fleetRepository = fleetRepository;
        this.queue = queue;
    }

    public async Task<CommandResponse> CreateCommandAsync(
        CreateCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FleetId))
            throw new ArgumentException("Fleet id is required");

        if (!Enum.IsDefined(request.Type))
            throw new ArgumentException("Command type is invalid");

        fleetRepository.GetOrThrow(request.FleetId);

        var command = new Command
        {
            Id = Guid.NewGuid().ToString(),
            Type = request.Type,
            Status = CommandStatus.Queued,
            Payload = new Dictionary<string, object?>
            {
                [FleetIdPayloadKey] = request.FleetId
            }
        };

        commandRepository.Create(command);

        await queue.QueueAsync(command.Id, cancellationToken);

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