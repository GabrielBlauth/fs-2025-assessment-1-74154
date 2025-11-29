using fs_2025_assessment_1_74154.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace fs_2025_assessment_1_74154.Services
{
    public class CosmoStationService : IStationService
    {
        private readonly Container? _container;
        private readonly List<Station> _fallbackStations = new();

        public CosmoStationService(CosmosClient? cosmosClient, IConfiguration config)
        {
            if (cosmosClient != null)
            {
                try
                {
                    var databaseId = config["CosmosDb:DatabaseId"] ?? "DublinBikesDb";
                    var containerId = config["CosmosDb:ContainerId"] ?? "Stations";
                    _container = cosmosClient.GetContainer(databaseId, containerId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error configuring CosmosDB: {ex.Message}");
                    _container = null;
                }
            }

            // Load fallback data
            LoadFallbackData();
        }

        private void LoadFallbackData()
        {
            // Carregar alguns dados de exemplo para fallback
            _fallbackStations.AddRange(new[]
            {
                new Station {
                    Number = 1,
                    Name = "Fallback Station 1",
                    Address = "Fallback Address 1",
                    BikeStands = 20,
                    AvailableBikes = 15,
                    AvailableBikeStands = 5,
                    Status = "OPEN",
                    LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    id = "1"
                },
                new Station {
                    Number = 2,
                    Name = "Fallback Station 2",
                    Address = "Fallback Address 2",
                    BikeStands = 15,
                    AvailableBikes = 10,
                    AvailableBikeStands = 5,
                    Status = "OPEN",
                    LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    id = "2"
                }
            });
        }

        // SYNC METHODS
        public List<Station> GetAllStations()
        {
            return GetAllStationsAsync().GetAwaiter().GetResult();
        }

        public Station? GetStationByNumber(int number)
        {
            return GetStationByNumberAsync(number).GetAwaiter().GetResult();
        }

        public StationSummary GetSummary()
        {
            return GetSummaryAsync().GetAwaiter().GetResult();
        }

        public void UpdateStation(Station station)
        {
            UpdateStationAsync(station).GetAwaiter().GetResult();
        }

        public void AddStation(Station station)
        {
            CreateStationAsync(station).GetAwaiter().GetResult();
        }

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

        public StationSummary GetCachedSummary()
        {
            return GetSummary();
        }

        // ASYNC METHODS FOR COSMOSDB
        public async Task<List<Station>> GetAllStationsAsync()
        {
            if (_container != null)
            {
                try
                {
                    var query = _container.GetItemQueryIterator<Station>();
                    var results = new List<Station>();

                    while (query.HasMoreResults)
                    {
                        var response = await query.ReadNextAsync();
                        results.AddRange(response);
                    }

                    return results;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CosmosDB error, using fallback: {ex.Message}");
                }
            }

            // Fallback to in-memory data
            return _fallbackStations;
        }

        public async Task<Station?> GetStationByNumberAsync(int number)
        {
            if (_container != null)
            {
                try
                {
                    var query = _container.GetItemLinqQueryable<Station>()
                        .Where(s => s.Number == number)
                        .Take(1)
                        .ToFeedIterator();

                    var response = await query.ReadNextAsync();
                    return response.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CosmosDB error, using fallback: {ex.Message}");
                }
            }

            // Fallback to in-memory data
            return _fallbackStations.FirstOrDefault(s => s.Number == number);
        }

        public async Task<Station> CreateStationAsync(Station station)
        {
            if (station == null)
                throw new ArgumentNullException(nameof(station));

            station.id = station.Number.ToString();

            if (_container != null)
            {
                try
                {
                    var response = await _container.CreateItemAsync(station);
                    return response.Resource;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CosmosDB error, using fallback: {ex.Message}");
                }
            }

            // Fallback to in-memory data
            if (!_fallbackStations.Any(s => s.Number == station.Number))
            {
                _fallbackStations.Add(station);
            }
            return station;
        }

        public async Task<Station> UpdateStationAsync(Station station)
        {
            if (station == null)
                throw new ArgumentNullException(nameof(station));

            if (string.IsNullOrEmpty(station.id))
                station.id = station.Number.ToString();

            if (_container != null)
            {
                try
                {
                    var response = await _container.UpsertItemAsync(station);
                    return response.Resource;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CosmosDB error, using fallback: {ex.Message}");
                }
            }

            // Fallback to in-memory data
            var existing = _fallbackStations.FirstOrDefault(s => s.Number == station.Number);
            if (existing != null)
            {
                _fallbackStations.Remove(existing);
            }
            _fallbackStations.Add(station);
            return station;
        }

        public async Task<bool> DeleteStationAsync(int number)
        {
            if (_container != null)
            {
                try
                {
                    var station = await GetStationByNumberAsync(number);
                    if (station == null || string.IsNullOrEmpty(station.id))
                        return false;

                    await _container.DeleteItemAsync<Station>(station.id, new PartitionKey(number.ToString()));
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CosmosDB error, using fallback: {ex.Message}");
                }
            }

            // Fallback to in-memory data
            var stationToRemove = _fallbackStations.FirstOrDefault(s => s.Number == number);
            if (stationToRemove != null)
            {
                _fallbackStations.Remove(stationToRemove);
                return true;
            }
            return false;
        }

        public async Task<StationSummary> GetSummaryAsync()
        {
            var stations = await GetAllStationsAsync();

            return new StationSummary
            {
                TotalStations = stations.Count,
                TotalBikeStands = stations.Sum(s => s.BikeStands),
                TotalAvailableBikes = stations.Sum(s => s.AvailableBikes),
                OpenStations = stations.Count(s => s.Status == "OPEN"),
                ClosedStations = stations.Count(s => s.Status == "CLOSED")
            };
        }
    }
}