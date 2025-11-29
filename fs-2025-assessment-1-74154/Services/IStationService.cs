using fs_2025_assessment_1_74154.Models;

namespace fs_2025_assessment_1_74154.Services
{
    public interface IStationService
    {
        List<Station> GetAllStations();
        Station? GetStationByNumber(int number);
        StationSummary GetSummary();
        void UpdateStation(Station station);
        void AddStation(Station station);

   
        List<Station> GetFilteredStations(string? status = null, int? minBikes = null, string? search = null);
        StationSummary GetCachedSummary();

        // METHODS FOR COSMOSDB COMPATIBILITY
        Task<List<Station>> GetAllStationsAsync();
        Task<Station> GetStationByNumberAsync(int number);
        Task<Station> CreateStationAsync(Station station);
        Task<Station> UpdateStationAsync(Station station);
        Task<bool> DeleteStationAsync(int number);
        Task<StationSummary> GetSummaryAsync();
    }

    public class StationSummary
    {
        public int TotalStations { get; set; }
        public int TotalBikeStands { get; set; }
        public int TotalAvailableBikes { get; set; }
        public int OpenStations { get; set; }
        public int ClosedStations { get; set; }
    }
}