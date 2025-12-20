using fs_2025_assessment_1_74154.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace fs_2025_assessment_1_74154.Services
{
    public class StationService : IStationService
    {
        private List<Station> _stations = new();
        private readonly string _jsonFilePath;
        private readonly IMemoryCache _cache;

        private const string SummaryCacheKey = "stations_summary";

        public StationService(IMemoryCache cache)
        {
            _jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "dublinbike.json");
            _cache = cache;
            LoadStations();
        }

        private void LoadStations()
        {
            try
            {
                var json = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };
                _stations = JsonSerializer.Deserialize<List<Station>>(json, options) ?? new List<Station>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading stations: {ex.Message}");
                _stations = new List<Station>();
            }
        }

        public List<Station> GetAllStations() => _stations;

        public Station? GetStationByNumber(int number) =>
            _stations.FirstOrDefault(s => s.Number == number);

        public StationSummary GetSummary()
        {
            var stations = _stations;

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

        public StationSummary GetCachedSummary()
        {
            return _cache.GetOrCreate(SummaryCacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return GetSummary();
            });
        }

        public List<Station> GetFilteredStations(string? status = null, int? minBikes = null, string? search = null)
        {
            var query = _stations.AsEnumerable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(s =>
                    string.Equals(s.Status, status, StringComparison.OrdinalIgnoreCase));

            if (minBikes.HasValue)
                query = query.Where(s => s.AvailableBikes >= minBikes.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(s =>
                    (s.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (s.Address ?? "").Contains(search, StringComparison.OrdinalIgnoreCase));

            return query.ToList();
        }

        public void UpdateStation(Station updatedStation)
        {
            var existing = _stations.FirstOrDefault(s => s.Number == updatedStation.Number);
            if (existing != null)
            {
                existing.Name = updatedStation.Name;
                existing.Address = updatedStation.Address;
                existing.Position = updatedStation.Position;
                existing.BikeStands = updatedStation.BikeStands;
                existing.AvailableBikes = updatedStation.AvailableBikes;
                existing.AvailableBikeStands = updatedStation.AvailableBikeStands;
                existing.Status = updatedStation.Status;
                existing.LastUpdate = updatedStation.LastUpdate;

                _cache.Remove(SummaryCacheKey);
            }
        }

        public void AddStation(Station station)
        {
            if (!_stations.Any(s => s.Number == station.Number))
            {
                _stations.Add(station);
                _cache.Remove(SummaryCacheKey);
            }
        }

        // -----------------------------
        // ASYNC METHODS (for interface compatibility)
        // -----------------------------
        public Task<List<Station>> GetAllStationsAsync()
        {
            return Task.FromResult(_stations);
        }

        public Task<Station?> GetStationByNumberAsync(int number)
        {
            return Task.FromResult(_stations.FirstOrDefault(s => s.Number == number));
        }

        public Task<Station> CreateStationAsync(Station station)
        {
            if (!_stations.Any(s => s.Number == station.Number))
            {
                _stations.Add(station);
                _cache.Remove(SummaryCacheKey);
            }
            return Task.FromResult(station);
        }

        public Task<Station> UpdateStationAsync(Station station)
        {
            var existing = _stations.FirstOrDefault(s => s.Number == station.Number);
            if (existing != null)
            {
                existing.Name = station.Name;
                existing.Address = station.Address;
                existing.Position = station.Position;
                existing.BikeStands = station.BikeStands;
                existing.AvailableBikes = station.AvailableBikes;
                existing.AvailableBikeStands = station.AvailableBikeStands;
                existing.Status = station.Status;
                existing.LastUpdate = station.LastUpdate;

                _cache.Remove(SummaryCacheKey);
            }
            return Task.FromResult(existing ?? station);
        }

        public Task<bool> DeleteStationAsync(int number)
        {
            var station = _stations.FirstOrDefault(s => s.Number == number);
            if (station != null)
            {
                _stations.Remove(station);
                _cache.Remove(SummaryCacheKey);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<StationSummary> GetSummaryAsync()
        {
            return Task.FromResult(GetSummary());
        }
    }
}
