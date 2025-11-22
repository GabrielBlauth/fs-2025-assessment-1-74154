using fs_2025_assessment_1_74154.Models;

namespace fs_2025_assessment_1_74154.Endpoints;

public static class Stations
{
    private static List<Station> _stations = new()
    {
        new()
        {
            Number = 42,
            Name = "SMITHFIELD NORTH",
            Address = "Smithfield North",
            Position = new Position { Lat = 53.349562, Lng = -6.278198 },
            BikeStands = 30,
            AvailableBikeStands = 15,
            AvailableBikes = 15,
            Status = "OPEN"
        },
        new()
        {
            Number = 30,
            Name = "PARNELL SQUARE",
            Address = "Parnell Square North",
            Position = new Position { Lat = 53.353741, Lng = -6.265301 },
            BikeStands = 20,
            AvailableBikeStands = 8,
            AvailableBikes = 12,
            Status = "OPEN"
        },
        new()
        {
            Number = 54,
            Name = "CLONMEL STREET",
            Address = "Clonmel Street",
            Position = new Position { Lat = 53.336021, Lng = -6.26298 },
            BikeStands = 33,
            AvailableBikeStands = 22,
            AvailableBikes = 11,
            Status = "OPEN"
        }
    };

    public static void MapStationsEndpoints(this WebApplication app)
    {
        // V1 - File based JSON
        var v1 = app.MapGroup("/api/v1/");
        v1.MapGet("stations", GetAllStations);
        v1.MapGet("stations/{number:int}", GetStationByNumber);
        v1.MapPost("stations", CreateStation);
        v1.MapPut("stations/{number:int}", UpdateStation);
        v1.MapGet("stations/search", SearchStations);

        // V2 - CosmosDB (Placeholder)
        var v2 = app.MapGroup("/api/v2/");
        v2.MapGet("stations", () => "V2 - CosmosDB implementation");
    }

    private static IResult GetAllStations()
    {
        return Results.Ok(_stations);
    }

    private static IResult GetStationByNumber(int number)
    {
        var station = _stations.FirstOrDefault(s => s.Number == number);
        if (station is null)
        {
            return Results.NotFound();
        }
        return Results.Ok(station);
    }

    private static IResult CreateStation(Station station)
    {
        // Remove if exists
        _stations.RemoveAll(s => s.Number == station.Number);

        // Add new station
        _stations.Add(station);

        return Results.Ok(station);
    }

    private static IResult UpdateStation(int number, Station updatedStation)
    {
        var existing = _stations.FirstOrDefault(s => s.Number == number);
        if (existing is null)
        {
            return Results.NotFound();
        }

        // Update properties
        existing.Name = updatedStation.Name;
        existing.Address = updatedStation.Address;
        existing.Position = updatedStation.Position;
        existing.BikeStands = updatedStation.BikeStands;
        existing.AvailableBikeStands = updatedStation.AvailableBikeStands;
        existing.AvailableBikes = updatedStation.AvailableBikes;
        existing.Status = updatedStation.Status;
        existing.LastUpdate = updatedStation.LastUpdate;

        return Results.Ok(existing);
    }

    private static IResult SearchStations(string? q)
    {
        var results = _stations.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            results = results.Where(s =>
                s.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                s.Address.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return Results.Ok(results.ToList());
    }
}