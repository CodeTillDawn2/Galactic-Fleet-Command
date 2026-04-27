using System.Net;
using System.Net.Http.Json;
using GalacticFleetCommand.Api.Contracts.Fleets;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GalacticFleetCommand.Tests.Integration;

public class FleetControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    private const string FleetsPath = "/fleets";

    private static string FleetPath(string id) => $"{FleetsPath}/{id}";

    public FleetControllerTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task PostFleet_ReturnsCreatedFleet()
    {
        var request = new CreateFleetRequest
        {
            Name = "Alpha",
            ShipCount = 5,
            FuelRequired = 100
        };

        var response = await client.PostAsJsonAsync(FleetsPath, request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var fleet = await response.Content.ReadFromJsonAsync<FleetResponse>();

        Assert.NotNull(fleet);
        Assert.False(string.IsNullOrWhiteSpace(fleet.Id));
        Assert.Equal("Alpha", fleet.Name);
        Assert.Equal(5, fleet.ShipCount);
        Assert.Equal(100, fleet.FuelRequired);
        Assert.Equal("Docked", fleet.State);
    }

    [Fact]
    public async Task PostFleet_WithInvalidInput_ReturnsBadRequest()
    {
        var request = new CreateFleetRequest
        {
            Name = "",
            ShipCount = 0,
            FuelRequired = -1
        };

        var response = await client.PostAsJsonAsync(FleetsPath, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetFleet_WhenFleetExists_ReturnsFleet()
    {
        var created = await CreateFleetAsync("Beta", 3, 50);

        var response = await client.GetAsync(FleetPath(created.Id));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fleet = await response.Content.ReadFromJsonAsync<FleetResponse>();

        Assert.NotNull(fleet);
        Assert.Equal(created.Id, fleet.Id);
        Assert.Equal("Beta", fleet.Name);
    }

    [Fact]
    public async Task GetFleet_WhenFleetDoesNotExist_ReturnsNotFound()
    {
        var response = await client.GetAsync(FleetPath(Guid.NewGuid().ToString()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PatchFleet_UpdatesEditableProperties()
    {
        var created = await CreateFleetAsync("Gamma", 4, 80);

        var request = new UpdateFleetRequest
        {
            Name = "Gamma Prime",
            ShipCount = 6,
            FuelRequired = 120
        };

        var response = await client.PatchAsJsonAsync(FleetPath(created.Id), request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fleet = await response.Content.ReadFromJsonAsync<FleetResponse>();

        Assert.NotNull(fleet);
        Assert.Equal(created.Id, fleet.Id);
        Assert.Equal("Gamma Prime", fleet.Name);
        Assert.Equal(6, fleet.ShipCount);
        Assert.Equal(120, fleet.FuelRequired);
        Assert.Equal("Docked", fleet.State);
    }

    [Fact]
    public async Task PatchFleet_WithInvalidInput_ReturnsBadRequest()
    {
        var created = await CreateFleetAsync("Delta", 2, 40);

        var request = new UpdateFleetRequest
        {
            ShipCount = 0
        };

        var response = await client.PatchAsJsonAsync(FleetPath(created.Id), request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PatchFleet_WhenFleetDoesNotExist_ReturnsNotFound()
    {
        var request = new UpdateFleetRequest
        {
            Name = "Missing"
        };

        var response = await client.PatchAsJsonAsync(FleetPath(Guid.NewGuid().ToString()), request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<FleetResponse> CreateFleetAsync(string name, int shipCount, int fuelRequired)
    {
        var request = new CreateFleetRequest
        {
            Name = name,
            ShipCount = shipCount,
            FuelRequired = fuelRequired
        };

        var response = await client.PostAsJsonAsync(FleetsPath, request);
        response.EnsureSuccessStatusCode();

        var fleet = await response.Content.ReadFromJsonAsync<FleetResponse>();

        return fleet ?? throw new InvalidOperationException("Create fleet response body was empty.");
    }
}