using fs_2025_assessment_1_74154.Helpers;
using fs_2025_assessment_1_74154.Models;
using fs_2025_assessment_1_74154.Services;

namespace fs_2025_assessment_1_74154.Extensions
{
    public static class StationEndpoints
    {
        public static void MapStationEndpoints(this WebApplication app)
        {
            // GET /api/stations
            app.MapGet("/api/stations", (
                IStationService stationService,
                string? status,
                int? minBikes,
                string? search,
                string sort = "name",
                string dir = "asc",
                int page = 1,
                int pageSize = 10
            ) =>
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;

                // Get all stations from the current backend (V1 = file, V2 = Cosmos)
                var stations = stationService.GetAllStations();
                var query = stations.AsEnumerable();

                // STATUS filter: OPEN / CLOSED (case-insensitive, trimmed)
                if (!string.IsNullOrWhiteSpace(status))
                {
                    var normalized = status.Trim();
                    query = query.Where(s =>
                        !string.IsNullOrWhiteSpace(s.Status) &&
                        string.Equals(s.Status.Trim(), normalized, StringComparison.OrdinalIgnoreCase));
                }

                // MIN BIKES filter
                if (minBikes.HasValue)
                {
                    query = query.Where(s => s.AvailableBikes >= minBikes.Value);
                }

                // SEARCH filter: looks in Name OR Address, case-insensitive
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.Trim();
                    query = query.Where(s =>
                        (!string.IsNullOrEmpty(s.Name) &&
                         s.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                        ||
                        (!string.IsNullOrEmpty(s.Address) &&
                         s.Address.Contains(term, StringComparison.OrdinalIgnoreCase)));
                }

                // Apply sorting using your helper
                var sorted = StationSortingHelper.Sort(query, sort, dir).ToList();

                var totalItems = sorted.Count;
                var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));

                if (page > totalPages) page = totalPages;

                var skip = (page - 1) * pageSize;

                var items = sorted
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                var hasNext = page < totalPages;
                var hasPrevious = page > 1;

                return Results.Ok(new
                {
                    totalItems,
                    totalPages,
                    page,
                    pageSize,
                    hasNext,
                    hasPrevious,
                    items
                });
            });


            // GET /api/stations/{number}
            app.MapGet("/api/stations/{number:int}", (IStationService stationService, int number) =>
            {
                var station = stationService.GetStationByNumber(number);
                return station is null ? Results.NotFound() : Results.Ok(station);
            });

            // GET /api/stations/summary
            app.MapGet("/api/stations/summary", (IStationService stationService) =>
            {
                return Results.Ok(stationService.GetCachedSummary());
            });

            // POST /api/stations
            app.MapPost("/api/stations", (IStationService stationService, Station station) =>
            {
                station.id = station.Number.ToString();

                var errors = StationValidator.Validate(station);
                if (errors.Any())
                    return Results.BadRequest(new { errors });

                if (stationService.GetStationByNumber(station.Number) != null)
                    return Results.Conflict(new { message = $"Station {station.Number} already exists." });

                stationService.AddStation(station);
                return Results.Created($"/api/stations/{station.Number}", station);
            });

            // PUT /api/stations/{number}
            app.MapPut("/api/stations/{number:int}", (IStationService stationService, int number, Station updated) =>
            {
                var existing = stationService.GetStationByNumber(number);
                if (existing == null)
                    return Results.NotFound();

                updated.Number = number;
                updated.id = number.ToString();

                var errors = StationValidator.Validate(updated);
                if (errors.Any())
                    return Results.BadRequest(new { errors });

                stationService.UpdateStation(updated);
                return Results.Ok(updated);
            });

            // DELETE /api/stations/{number}
            app.MapDelete("/api/stations/{number:int}", async (IStationService stationService, int number) =>
            {
                var deleted = await stationService.DeleteStationAsync(number);
                return deleted ? Results.NoContent() : Results.NotFound();
            });
        }
    }
}
