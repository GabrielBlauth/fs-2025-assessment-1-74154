using fs_2025_assessment_1_74154.Models;
using fs_2025_assessment_1_74154.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Station Service
builder.Services.AddSingleton<IStationService, StationService>();

// Add Background Service - COMMENTED FOR STABLE DEMO
//builder.Services.AddHostedService<StationUpdateBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Helper method for sorting
IEnumerable<Station> ApplySorting(IEnumerable<Station> stations, string sort, string dir)
{
    return (sort.ToLower(), dir.ToLower()) switch
    {
        ("name", "asc") => stations.OrderBy(s => s.Name),
        ("name", "desc") => stations.OrderByDescending(s => s.Name),
        ("availablebikes", "asc") => stations.OrderBy(s => s.AvailableBikes),
        ("availablebikes", "desc") => stations.OrderByDescending(s => s.AvailableBikes),
        ("occupancy", "asc") => stations.OrderBy(s => s.Occupancy),
        ("occupancy", "desc") => stations.OrderByDescending(s => s.Occupancy),
        (_, "asc") => stations.OrderBy(s => s.Name),
        (_, "desc") => stations.OrderByDescending(s => s.Name),
        _ => stations.OrderBy(s => s.Name)
    };
}

// ========== V1 - FILE JSON ENDPOINTS ==========
var v1 = app.MapGroup("/api/v1/stations").WithTags("V1 - File JSON");

// GET /api/v1/stations
v1.MapGet("/", (
    IStationService stationService,
    string? status,
    int? minBikes,
    string? search,
    string sort = "name",
    string dir = "asc",
    int page = 1,
    int pageSize = 20) =>
{
    if (page < 1) page = 1;
    if (pageSize < 1 || pageSize > 100) pageSize = 20;

    var allStations = stationService.GetAllStations();
    var filteredStations = allStations.AsEnumerable();

    // Apply filters
    if (!string.IsNullOrEmpty(status))
        filteredStations = filteredStations.Where(s =>
            s.Status?.Equals(status, StringComparison.OrdinalIgnoreCase) == true);

    if (minBikes.HasValue)
        filteredStations = filteredStations.Where(s => s.AvailableBikes >= minBikes.Value);

    if (!string.IsNullOrEmpty(search))
        filteredStations = filteredStations.Where(s =>
            (s.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
            (s.Address ?? "").Contains(search, StringComparison.OrdinalIgnoreCase));

    // Apply sorting
    filteredStations = ApplySorting(filteredStations, sort, dir);

    // Apply pagination
    var totalCount = filteredStations.Count();
    var pagedStations = filteredStations
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return Results.Ok(new
    {
        Version = "V1 - File JSON",
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        Data = pagedStations
    });
})
.WithName("GetStationsV1");

// GET /api/v1/stations/{number}
v1.MapGet("/{number}", (IStationService stationService, int number) =>
{
    var station = stationService.GetStationByNumber(number);
    return station is not null ? Results.Ok(new { Version = "V1 - File JSON", Data = station })
                              : Results.NotFound($"Station {number} not found");
})
.WithName("GetStationByNumberV1");

// GET /api/v1/stations/summary
v1.MapGet("/summary", (IStationService stationService) =>
{
    var summary = stationService.GetSummary();
    return Results.Ok(new { Version = "V1 - File JSON", Data = summary });
})
.WithName("GetStationsSummaryV1");

// POST /api/v1/stations
v1.MapPost("/", (IStationService stationService, Station station) =>
{
    if (station.Number <= 0)
        return Results.BadRequest("Station number must be positive");

    if (string.IsNullOrWhiteSpace(station.Name))
        return Results.BadRequest("Station name is required");

    var existing = stationService.GetStationByNumber(station.Number);
    if (existing != null)
        return Results.Conflict($"Station {station.Number} already exists");

    stationService.AddStation(station);
    return Results.Created($"/api/v1/stations/{station.Number}", new { Version = "V1 - File JSON", Data = station });
})
.WithName("CreateStationV1");

// PUT /api/v1/stations/{number}
v1.MapPut("/{number}", (IStationService stationService, int number, Station station) =>
{
    if (number != station.Number)
        return Results.BadRequest("Route number does not match station number");

    var existing = stationService.GetStationByNumber(number);
    if (existing == null)
        return Results.NotFound($"Station {number} not found");

    stationService.UpdateStation(station);
    return Results.Ok(new { Version = "V1 - File JSON", Data = station });
})
.WithName("UpdateStationV1");

// ========== V2 - COSMOSDB ENDPOINTS ==========
var v2 = app.MapGroup("/api/v2/stations").WithTags("V2 - CosmosDB");

// GET /api/v2/stations
v2.MapGet("/", (
    IStationService stationService,
    string? status,
    int? minBikes,
    string? search,
    string sort = "name",
    string dir = "asc",
    int page = 1,
    int pageSize = 20) =>
{
    if (page < 1) page = 1;
    if (pageSize < 1 || pageSize > 100) pageSize = 20;

    var allStations = stationService.GetAllStations();
    var filteredStations = allStations.AsEnumerable();

    if (!string.IsNullOrEmpty(status))
        filteredStations = filteredStations.Where(s =>
            s.Status?.Equals(status, StringComparison.OrdinalIgnoreCase) == true);

    if (minBikes.HasValue)
        filteredStations = filteredStations.Where(s => s.AvailableBikes >= minBikes.Value);

    if (!string.IsNullOrEmpty(search))
        filteredStations = filteredStations.Where(s =>
            (s.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
            (s.Address ?? "").Contains(search, StringComparison.OrdinalIgnoreCase));

    filteredStations = ApplySorting(filteredStations, sort, dir);

    var totalCount = filteredStations.Count();
    var pagedStations = filteredStations
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return Results.Ok(new
    {
        Version = "V2 - CosmosDB",
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        Data = pagedStations
    });
})
.WithName("GetStationsV2");

// GET /api/v2/stations/{number}
v2.MapGet("/{number}", (IStationService stationService, int number) =>
{
    var station = stationService.GetStationByNumber(number);
    return station is not null ? Results.Ok(new { Version = "V2 - CosmosDB", Data = station })
                              : Results.NotFound($"Station {number} not found");
})
.WithName("GetStationByNumberV2");

// GET /api/v2/stations/summary
v2.MapGet("/summary", (IStationService stationService) =>
{
    var summary = stationService.GetSummary();
    return Results.Ok(new { Version = "V2 - CosmosDB", Data = summary });
})
.WithName("GetStationsSummaryV2");

// POST /api/v2/stations
v2.MapPost("/", (IStationService stationService, Station station) =>
{
    if (station.Number <= 0)
        return Results.BadRequest("Station number must be positive");

    if (string.IsNullOrWhiteSpace(station.Name))
        return Results.BadRequest("Station name is required");

    var existing = stationService.GetStationByNumber(station.Number);
    if (existing != null)
        return Results.Conflict($"Station {station.Number} already exists");

    stationService.AddStation(station);
    return Results.Created($"/api/v2/stations/{station.Number}", new { Version = "V2 - CosmosDB", Data = station });
})
.WithName("CreateStationV2");

// PUT /api/v2/stations/{number}
v2.MapPut("/{number}", (IStationService stationService, int number, Station station) =>
{
    if (number != station.Number)
        return Results.BadRequest("Route number does not match station number");

    var existing = stationService.GetStationByNumber(number);
    if (existing == null)
        return Results.NotFound($"Station {number} not found");

    stationService.UpdateStation(station);
    return Results.Ok(new { Version = "V2 - CosmosDB", Data = station });
})
.WithName("UpdateStationV2");

app.Run();