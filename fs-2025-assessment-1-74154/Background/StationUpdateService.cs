using fs_2025_assessment_1_74154.Services;

namespace fs_2025_assessment_1_74154.Background;

public class StationUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StationUpdateService> _logger;
    private readonly Random _random = new();

    public StationUpdateService(IServiceProvider serviceProvider, ILogger<StationUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Station Update Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var stationService = scope.ServiceProvider.GetRequiredService<IStationService>();

                UpdateStations(stationService);
                _logger.LogInformation("Stations updated with random data.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stations");
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); // Update every 15 seconds
        }
    }

    private void UpdateStations(IStationService stationService)
    {
        var stations = stationService.GetAllStations();

        foreach (var station in stations)
        {
            // Randomly update capacity and availability
            var newBikeStands = _random.Next(20, 41); // Between 20 and 40
            var newAvailableBikes = _random.Next(0, newBikeStands + 1);

            station.BikeStands = newBikeStands;
            station.AvailableBikes = newAvailableBikes;
            station.AvailableBikeStands = newBikeStands - newAvailableBikes;
            station.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            stationService.UpdateStation(station);
        }
    }
}