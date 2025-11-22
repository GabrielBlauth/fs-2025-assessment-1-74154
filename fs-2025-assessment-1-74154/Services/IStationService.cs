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
