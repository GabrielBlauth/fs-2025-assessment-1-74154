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
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                _stations = JsonSerializer.Deserialize<List<Station>>(json, options) ?? new List<Station>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading stations: {ex.Message}");
                _stations = new List<Station>();
            }
        }

        public List<Station> GetAllStations() => _stations;

        public Station? GetStationByNumber(int number) => _stations.FirstOrDefault(s => s.Number == number);

        public StationSummary GetSummary()
        {
            return new StationSummary
            {
                TotalStations = _stations.Count,
                TotalBikeStands = _stations.Sum(s => s.BikeStands),
                TotalAvailableBikes = _stations.Sum(s => s.AvailableBikes),
                OpenStations = _stations.Count(s => s.Status == "OPEN"),
                ClosedStations = _stations.Count(s => s.Status == "CLOSED")
            };
        }

        public List<Station> GetFilteredStations(string? status = null, int? minBikes = null, string? search = null)
        {
            var cacheKey = $"stations_{status}_{minBikes}_{search}";

            return _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                var query = _stations.AsEnumerable();

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(s => s.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

                if (minBikes.HasValue)
                    query = query.Where(s => s.AvailableBikes >= minBikes.Value);

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(s =>
                        (s.Name ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (s.Address ?? "").Contains(search, StringComparison.OrdinalIgnoreCase));

                return query.ToList();
            });
        }

        public StationSummary GetCachedSummary()
        {
            return _cache.GetOrCreate("stations_summary", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return GetSummary();
            });
        }

        public void UpdateStation(Station updatedStation)
        {
            var existing = _stations.FirstOrDefault(s => s.Number == updatedStation.Number);
            if (existing != null)
            {
                existing.AvailableBikes = updatedStation.AvailableBikes;
                existing.AvailableBikeStands = updatedStation.AvailableBikeStands;
                existing.LastUpdate = updatedStation.LastUpdate;
            }
        }

        public void AddStation(Station station)
        {
            if (!_stations.Any(s => s.Number == station.Number))
            {
                _stations.Add(station);
            }
        }
    }
}