using GalacticFleetCommand.Api.Contracts.Fleets;
using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Infrastructure;

namespace GalacticFleetCommand.Api.Application;

public class FleetService
{
    private readonly PersistenceContext context;

    public FleetService(PersistenceContext context)
    {
        this.context = context;
    }

    public FleetResponse Create(CreateFleetRequest request)
    {
        ValidateCreateRequest(request);

        var fleet = new Fleet
        {
            Id = Guid.NewGuid().ToString(),
            Version = 1,
            Name = request.Name.Trim(),
            ShipCount = request.ShipCount,
            FuelRequired = request.FuelRequired
        };

        context.Fleets.Create(fleet);

        return ToResponse(fleet);
    }

    public FleetResponse Get(string id)
    {
        var fleet = context.Fleets.GetOrThrow(id);

        return ToResponse(fleet);
    }

    public FleetResponse Update(string id, UpdateFleetRequest request)
    {
        var current = context.Fleets.GetOrThrow(id);

        ValidateUpdateRequest(request);

        context.Fleets.Update(id, current.Version, fleet =>
        {
            if (request.Name is not null)
                fleet.Name = request.Name.Trim();

            if (request.ShipCount.HasValue)
                fleet.ShipCount = request.ShipCount.Value;

            if (request.FuelRequired.HasValue)
                fleet.FuelRequired = request.FuelRequired.Value;

            return fleet;
        });

        return ToResponse(context.Fleets.GetOrThrow(id));
    }

    private static void ValidateCreateRequest(CreateFleetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Fleet name is required.");

        if (request.ShipCount <= 0)
            throw new ArgumentException("Ship count must be greater than zero.");

        if (request.FuelRequired < 0)
            throw new ArgumentException("Fuel required cannot be negative.");
    }

    private static void ValidateUpdateRequest(UpdateFleetRequest request)
    {
        if (request.Name is not null && string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Fleet name cannot be empty.");

        if (request.ShipCount.HasValue && request.ShipCount.Value <= 0)
            throw new ArgumentException("Ship count must be greater than zero.");

        if (request.FuelRequired.HasValue && request.FuelRequired.Value < 0)
            throw new ArgumentException("Fuel required cannot be negative.");
    }

    private static FleetResponse ToResponse(Fleet fleet)
    {
        return new FleetResponse
        {
            Id = fleet.Id,
            Name = fleet.Name,
            ShipCount = fleet.ShipCount,
            FuelRequired = fleet.FuelRequired,
            State = fleet.State.ToString(),
            Transitions = fleet.Transitions
                .Select(transition => new FleetTransitionResponse
                {
                    From = transition.From.ToString(),
                    To = transition.To.ToString(),
                    OccurredAtUtc = transition.OccurredAtUtc
                })
                .ToList()
        };
    }
}