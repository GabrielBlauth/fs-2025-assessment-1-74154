using fs_2025_assessment_1_74154.Models;

namespace fs_2025_assessment_1_74154.Services;

public class StationUpdateBackgroundService : BackgroundService
{
    private readonly ILogger<StationUpdateBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Random _random = new Random();

    public StationUpdateBackgroundService(
        ILogger<StationUpdateBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Station Update Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var stationService = scope.ServiceProvider.GetRequiredService<IStationService>();

                    // Update stations with random data every 30 seconds
                    await UpdateStationsRandomly(stationService);
                }

                // Wait for 30 seconds before next update
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Station Update Background Service.");
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken); // Wait longer on error
            }
        }
    }

    private async Task UpdateStationsRandomly(IStationService stationService)
    {
        var stations = stationService.GetAllStations();

        foreach (var station in stations)
        {
            // Skip closed stations
            if (station.Status == "CLOSED")
                continue;

            // Random changes to simulate real-world data
            var randomChange = _random.Next(-3, 4); // -3 to +3 bikes

            var newAvailableBikes = station.AvailableBikes + randomChange;
            var newAvailableStands = station.BikeStands - newAvailableBikes;

            // Ensure values are within valid range
            newAvailableBikes = Math.Max(0, Math.Min(station.BikeStands, newAvailableBikes));
            newAvailableStands = Math.Max(0, Math.Min(station.BikeStands, newAvailableStands));

            // Occasionally change station status (5% chance)
            if (_random.NextDouble() < 0.05)
            {
                station.Status = station.Status == "OPEN" ? "CLOSED" : "OPEN";
            }

            // Update station data
            station.AvailableBikes = newAvailableBikes;
            station.AvailableBikeStands = newAvailableStands;
            station.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Update via service
            stationService.UpdateStation(station);
        }

        _logger.LogInformation("Updated {Count} stations with random data at {Time}",
            stations.Count, DateTime.Now.ToString("HH:mm:ss"));
    }
}