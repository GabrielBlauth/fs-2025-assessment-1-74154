using fs_2025_assessment_1_74154.Models;
using fs_2025_assessment_1_74154.Services;
using fs_2025_assessment_1_74154_App.Models;

namespace fs_2025_assessment_1_74154_App.Services
{
    public interface IStationsApiClient
    {
        Task<PaginatedResult<Station>> GetStationsAsync(
            string? status,
            int? minBikes,
            string? search,
            string sort,
            string dir,
            int page,
            int pageSize);

        Task<Station?> GetStationAsync(int number);

        Task<StationSummaryDto?> GetSummaryAsync();

        Task<Station> CreateStationAsync(Station station);

        Task<Station> UpdateStationAsync(int number, Station station);

        Task DeleteStationAsync(int number);
    }
}
