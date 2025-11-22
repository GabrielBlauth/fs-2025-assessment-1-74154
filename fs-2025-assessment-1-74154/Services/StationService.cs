using fs_2025_assessment_1_74154.Models;
using System.Text.Json;

namespace fs_2025_assessment_1_74154.Services
{
    public class StationService : IStationService
    {
        private List<Station> _stations = new();
        private readonly string _jsonFilePath;

        public StationService()
        {
            _jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "dublinbikes.json");
            LoadStations();
        }

        private void LoadStations()
        {
            try
            {
                var json = File.ReadAllText(_jsonFilePath);
                _stations = JsonSerializer.Deserialize<List<Station>>(json) ?? new List<Station>();
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
            return new StationSummary
            {
                TotalStations = _stations.Count,
                TotalBikeStands = _stations.Sum(s => s.BikeStands),
                TotalAvailableBikes = _stations.Sum(s => s.AvailableBikes),
                OpenStations = _stations.Count(s => s.Status == "OPEN"),
                ClosedStations = _stations.Count(s => s.Status == "CLOSED")
            };
        }

        public void UpdateStation(Station station)
        {
            var existing = _stations.FirstOrDefault(s => s.Number == station.Number);
            if (existing != null)
            {
                _stations.Remove(existing);
                _stations.Add(station);
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
