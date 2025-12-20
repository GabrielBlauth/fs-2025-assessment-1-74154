using fs_2025_assessment_1_74154.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Text.Json;

namespace fs_2025_assessment_1_74154.Services
{
    public class CosmoStationService : IStationService
    {
        private readonly Container? _container;
        private readonly IConfiguration _config;

        public CosmoStationService(CosmosClient? cosmosClient, IConfiguration config)
        {
            _config = config;

            if (cosmosClient != null)
            {
                try
                {
                    var databaseId = config["CosmosDb:DatabaseId"] ?? "DublinBikesDb";
                    var containerId = config["CosmosDb:ContainerId"] ?? "Stations";

                    var database = cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId)
                                               .GetAwaiter()
                                               .GetResult();

                    database.Database.CreateContainerIfNotExistsAsync(
                        new ContainerProperties
                        {
                            Id = containerId,
                            PartitionKeyPath = "/id"
                        })
                        .GetAwaiter()
                        .GetResult();

                    _container = cosmosClient.GetContainer(databaseId, containerId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CosmosDB init error: {ex}");
                    _container = null;
                }
            }
        }

        // -----------------------------
        // JSON LOADING & DB SEEDING
        // -----------------------------
        private async Task<List<Station>> LoadStationsFromJsonAsync()
        {
            var filePath = Path.Combine(
                AppContext.BaseDirectory,
                "Data",
                "dublinbike.json"
            );

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("JSON data file not found.", filePath);
            }

            var json = await File.ReadAllTextAsync(filePath);

            // IMPORTANT: use the same options as V1 (snake_case to C# props)
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            var stations = JsonSerializer.Deserialize<List<Station>>(json, options);

            if (stations == null)
                throw new Exception("Failed to deserialize station data.");

            foreach (var station in stations)
            {
                station.id = station.Number.ToString();
            }

            return stations;
        }

        private async Task EnsureDatabaseSeededAsync()
        {
            if (_container == null)
                return;

            // DO NOT call GetAllStationsAsync here (avoids infinite recursion)
            var hasData = false;

            var iterator = _container.GetItemQueryIterator<Station>(
                new QueryDefinition("SELECT TOP 1 * FROM c")
            );

            if (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                hasData = response.Any();
            }

            if (hasData)
                return;

            var stationsFromJson = await LoadStationsFromJsonAsync();

            foreach (var station in stationsFromJson)
            {
                await _container.UpsertItemAsync(
                    station,
                    new PartitionKey(station.id)
                );
            }
        }

        // -----------------------------
        // SYNC WRAPPERS
        // -----------------------------
        public List<Station> GetAllStations() =>
            GetAllStationsAsync().GetAwaiter().GetResult();

        public Station? GetStationByNumber(int number) =>
            GetStationByNumberAsync(number).GetAwaiter().GetResult();

        public StationSummary GetSummary() =>
            GetSummaryAsync().GetAwaiter().GetResult();

        public StationSummary GetCachedSummary()
        {
            // No IMemoryCache here yet – just delegate to GetSummary()
            return GetSummary();
        }

        public void UpdateStation(Station station) =>
            UpdateStationAsync(station).GetAwaiter().GetResult();

        public void AddStation(Station station) =>
            CreateStationAsync(station).GetAwaiter().GetResult();

        public List<Station> GetFilteredStations(string? status = null, int? minBikes = null, string? search = null)
        {
            var stations = GetAllStations();
            var query = stations.AsEnumerable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(s => s.Status?.Equals(status, StringComparison.OrdinalIgnoreCase) == true);

            if (minBikes.HasValue)
                query = query.Where(s => s.AvailableBikes >= minBikes.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(s =>
                    (s.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (s.Address ?? "").Contains(search, StringComparison.OrdinalIgnoreCase));

            return query.ToList();
        }

        // -----------------------------
        // ASYNC COSMOS METHODS
        // -----------------------------
        public async Task<List<Station>> GetAllStationsAsync()
        {
            if (_container == null)
                return await LoadStationsFromJsonAsync();

            await EnsureDatabaseSeededAsync();

            var results = new List<Station>();
            var query = _container.GetItemQueryIterator<Station>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }

        public async Task<Station?> GetStationByNumberAsync(int number)
        {
            if (_container == null)
                return (await LoadStationsFromJsonAsync())
                    .FirstOrDefault(s => s.Number == number);

            var query = _container.GetItemLinqQueryable<Station>()
                                  .Where(s => s.Number == number)
                                  .Take(1)
                                  .ToFeedIterator();

            var response = await query.ReadNextAsync();
            return response.FirstOrDefault();
        }

        public async Task<Station> CreateStationAsync(Station station)
        {
            if (station == null)
                throw new ArgumentNullException(nameof(station));

            station.id = station.Number.ToString();

            if (_container != null)
            {
                var response = await _container.CreateItemAsync(
                    station,
                    new PartitionKey(station.id)
                );
                return response.Resource;
            }

            return station;
        }

        public async Task<Station> UpdateStationAsync(Station station)
        {
            if (station == null)
                throw new ArgumentNullException(nameof(station));

            station.id ??= station.Number.ToString();

            if (_container != null)
            {
                var response = await _container.UpsertItemAsync(
                    station,
                    new PartitionKey(station.id)
                );
                return response.Resource;
            }

            return station;
        }

        public async Task<bool> DeleteStationAsync(int number)
        {
            if (_container == null)
                return false;

            var id = number.ToString();
            await _container.DeleteItemAsync<Station>(
                id,
                new PartitionKey(id)
            );

            return true;
        }

        public async Task<StationSummary> GetSummaryAsync()
        {
            var stations = await GetAllStationsAsync();

            var totalStations = stations.Count;
            var totalBikeStands = stations.Sum(s => s.BikeStands);
            var totalAvailableBikes = stations.Sum(s => s.AvailableBikes);

            var openStations = stations.Count(s =>
                string.Equals(s.Status, "OPEN", StringComparison.OrdinalIgnoreCase));

            var closedStations = stations.Count(s =>
                string.Equals(s.Status, "CLOSED", StringComparison.OrdinalIgnoreCase));

            return new StationSummary
            {
                TotalStations = totalStations,
                TotalBikeStands = totalBikeStands,
                TotalAvailableBikes = totalAvailableBikes,
                OpenStations = openStations,
                ClosedStations = closedStations
            };
        }
    }
}
