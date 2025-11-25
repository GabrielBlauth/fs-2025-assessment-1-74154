using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using fs_2025_assessment_1_74154.Models;

namespace fs_2025_assessment_1_74154.Services;

public class StationUpdateBackgroundService : BackgroundService
{
    private readonly ILogger<StationUpdateBackgroundService> _logger;
    private readonly IStationService _stationService;
    private readonly Random _random = new Random();

    public StationUpdateBackgroundService(
        ILogger<StationUpdateBackgroundService> logger,
        IStationService stationService)
    {
        _logger = logger;
        _stationService = stationService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Station Update Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                UpdateStationsRandomly();
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stations");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        _logger.LogInformation("Station Update Background Service stopped.");
    }

    private void UpdateStationsRandomly()
    {
        var stations = _stationService.GetAllStations();

        foreach (var station in stations)
        {
            if (station.Status != "OPEN") continue;

            var change = _random.Next(-3, 4);
            var newBikes = station.AvailableBikes + change;
            newBikes = Math.Max(0, Math.Min(station.BikeStands, newBikes));

            station.AvailableBikes = newBikes;
            station.AvailableBikeStands = station.BikeStands - newBikes;
            station.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        _logger.LogDebug("Updated stations availability");
    }
}