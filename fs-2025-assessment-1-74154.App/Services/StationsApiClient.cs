using fs_2025_assessment_1_74154.Models;
using fs_2025_assessment_1_74154.Services;
using fs_2025_assessment_1_74154_App.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace fs_2025_assessment_1_74154_App.Services
{
    public class StationsApiClient : IStationsApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public StationsApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<PaginatedResult<Station>> GetStationsAsync(
            string? status,
            int? minBikes,
            string? search,
            string sort,
            string dir,
            int page,
            int pageSize)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(status))
                queryParams.Add($"status={Uri.EscapeDataString(status)}");

            if (minBikes.HasValue)
                queryParams.Add($"minBikes={minBikes.Value}");

            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrWhiteSpace(sort))
                queryParams.Add($"sort={Uri.EscapeDataString(sort)}");

            if (!string.IsNullOrWhiteSpace(dir))
                queryParams.Add($"dir={Uri.EscapeDataString(dir)}");

            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var queryString = string.Join("&", queryParams);
            var url = "/api/stations";
            if (!string.IsNullOrEmpty(queryString))
                url += "?" + queryString;

            var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error {response.StatusCode}: {body}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<PaginatedResult<Station>>(stream, _jsonOptions);

            return result ?? new PaginatedResult<Station>();
        }

        public async Task<Station?> GetStationAsync(int number)
        {
            var response = await _http.GetAsync($"/api/stations/{number}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error {response.StatusCode}: {body}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<Station>(stream, _jsonOptions);
        }

        public async Task<StationSummaryDto?> GetSummaryAsync()
        {
            var response = await _http.GetAsync("/api/stations/summary");

            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<StationSummaryDto>(stream, _jsonOptions);
        }

        public async Task<Station> CreateStationAsync(Station station)
        {
            var response = await _http.PostAsJsonAsync("/api/stations", station);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                var problem = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(problem);
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error {response.StatusCode}: {body}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var created = await JsonSerializer.DeserializeAsync<Station>(stream, _jsonOptions);
            return created ?? station;
        }

        public async Task<Station> UpdateStationAsync(int number, Station station)
        {
            var response = await _http.PutAsJsonAsync($"/api/stations/{number}", station);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var problem = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(problem);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException($"Station {number} not found.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error {response.StatusCode}: {body}");
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var updated = await JsonSerializer.DeserializeAsync<Station>(stream, _jsonOptions);
            return updated ?? station;
        }

        public async Task DeleteStationAsync(int number)
        {
            var response = await _http.DeleteAsync($"/api/stations/{number}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new InvalidOperationException($"Station {number} not found.");

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"API error {response.StatusCode}: {body}");
            }
        }
    }
}
