using GalacticFleetCommand.Api.Application;
using GalacticFleetCommand.Api.Contracts.Fleets;
using GalacticFleetCommand.Api.Domain;
using GalacticFleetCommand.Api.Infrastructure;

namespace GalacticFleetCommand.Tests.Unit;

public class FleetServiceTests
{
    [Fact]
    public void Create_WithValidRequest_CreatesDockedFleet()
    {
        var service = CreateService();

        var fleet = service.Create(new CreateFleetRequest
        {
            Name = "Alpha",
            ShipCount = 5,
            FuelRequired = 100
        });

        Assert.False(string.IsNullOrWhiteSpace(fleet.Id));
        Assert.Equal("Alpha", fleet.Name);
        Assert.Equal(5, fleet.ShipCount);
        Assert.Equal(100, fleet.FuelRequired);
        Assert.Equal("Docked", fleet.State);
    }

    [Fact]
    public void Create_TrimsFleetName()
    {
        var service = CreateService();

        var fleet = service.Create(new CreateFleetRequest
        {
            Name = "  Alpha  ",
            ShipCount = 5,
            FuelRequired = 100
        });

        Assert.Equal("Alpha", fleet.Name);
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        var service = CreateService();

        var ex = Assert.Throws<ArgumentException>(() =>
            service.Create(new CreateFleetRequest
            {
                Name = "",
                ShipCount = 5,
                FuelRequired = 100
            }));

        Assert.Equal("Fleet name is required.", ex.Message);
    }

    [Fact]
    public void Create_WithNonPositiveShipCount_ThrowsArgumentException()
    {
        var service = CreateService();

        var ex = Assert.Throws<ArgumentException>(() =>
            service.Create(new CreateFleetRequest
            {
                Name = "Alpha",
                ShipCount = 0,
                FuelRequired = 100
            }));

        Assert.Equal("Ship count must be greater than zero.", ex.Message);
    }

    [Fact]
    public void Create_WithNegativeFuelRequired_ThrowsArgumentException()
    {
        var service = CreateService();

        var ex = Assert.Throws<ArgumentException>(() =>
            service.Create(new CreateFleetRequest
            {
                Name = "Alpha",
                ShipCount = 5,
                FuelRequired = -1
            }));

        Assert.Equal("Fuel required cannot be negative.", ex.Message);
    }

    [Fact]
    public void Get_WhenFleetExists_ReturnsFleet()
    {
        var context = new PersistenceContext();
        var service = new FleetService(context);

        var created = service.Create(new CreateFleetRequest
        {
            Name = "Alpha",
            ShipCount = 5,
            FuelRequired = 100
        });

        var fleet = service.Get(created.Id);

        Assert.Equal(created.Id, fleet.Id);
        Assert.Equal("Alpha", fleet.Name);
    }

    [Fact]
    public void Get_WhenFleetDoesNotExist_ThrowsNotFoundException()
    {
        var service = CreateService();

        Assert.Throws<NotFoundException>(() => service.Get(Guid.NewGuid().ToString()));
    }

    [Fact]
    public void Update_UpdatesProvidedFieldsOnly()
    {
        var context = new PersistenceContext();
        var service = new FleetService(context);

        var created = service.Create(new CreateFleetRequest
        {
            Name = "Alpha",
            ShipCount = 5,
            FuelRequired = 100
        });

        var updated = service.Update(created.Id, new UpdateFleetRequest
        {
            Name = "Alpha Prime"
        });

        Assert.Equal("Alpha Prime", updated.Name);
        Assert.Equal(5, updated.ShipCount);
        Assert.Equal(100, updated.FuelRequired);
        Assert.Equal("Docked", updated.State);
    }

    [Fact]
    public void Update_TrimsFleetName()
    {
        var context = new PersistenceContext();
        var service = new FleetService(context);

        var created = service.Create(new CreateFleetRequest
        {
            Name = "Alpha",
            ShipCount = 5,
            FuelRequired = 100
        });

        var updated = service.Update(created.Id, new UpdateFleetRequest
        {
            Name = "  Alpha Prime  "
        });

        Assert.Equal("Alpha Prime", updated.Name);
    }

    [Fact]
    public void Update_WithEmptyName_ThrowsArgumentException()
    {
        var context = new PersistenceContext();
        var service = new FleetService(context);

        var created = service.Create(new CreateFleetRequest
        {
            Name = "Alpha",
            ShipCount = 5,
            FuelRequired = 100
        });

        var ex = Assert.Throws<ArgumentException>(() =>
            service.Update(created.Id, new UpdateFleetRequest
            {
                Name = ""
            }));

        Assert.Equal("Fleet name cannot be empty.", ex.Message);
    }

    [Fact]
    public void Update_WithNonPositiveShipCount_ThrowsArgumentException()
    {
        var context = new PersistenceContext();
        var service = new FleetService(context);

        var created = service.Create(new CreateFleetRequest
        {
            Name = "Alpha",
            ShipCount = 5,
            FuelRequired = 100
        });

        var ex = Assert.Throws<ArgumentException>(() =>
            service.Update(created.Id, new UpdateFleetRequest
            {
                ShipCount = 0
            }));

        Assert.Equal("Ship count must be greater than zero.", ex.Message);
    }

    [Fact]
    public void Update_WithNegativeFuelRequired_ThrowsArgumentException()
    {
        var context = new PersistenceContext();
        var service = new FleetService(context);

        var created = service.Create(new CreateFleetRequest
        {
            Name = "Alpha",
            ShipCount = 5,
            FuelRequired = 100
        });

        var ex = Assert.Throws<ArgumentException>(() =>
            service.Update(created.Id, new UpdateFleetRequest
            {
                FuelRequired = -1
            }));

        Assert.Equal("Fuel required cannot be negative.", ex.Message);
    }

    [Fact]
    public void Update_WhenFleetDoesNotExist_ThrowsNotFoundException()
    {
        var service = CreateService();

        Assert.Throws<NotFoundException>(() =>
            service.Update(Guid.NewGuid().ToString(), new UpdateFleetRequest
            {
                Name = "Missing"
            }));
    }

    private static FleetService CreateService()
    {
        return new FleetService(new PersistenceContext());
    }
}