using fs_2025_assessment_1_74154.Services;
using Microsoft.Azure.Cosmos;

namespace fs_2025_assessment_1_74154;

public static class DependenciesConfig
{
    public static void ConfigureServices(WebApplicationBuilder builder, string version)
    {
        // Serviços comuns para ambas versões
        builder.Services.AddMemoryCache();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configurar serviço baseado na versão
        if (version == "V1")
        {
            ConfigureV1Services(builder);
        }
        else if (version == "V2")
        {
            ConfigureV2Services(builder);
        }

        // Background service - COMMENTED FOR NOW
        // builder.Services.AddHostedService<BackgroundDataUpdateService>();
    }

    private static void ConfigureV1Services(WebApplicationBuilder builder)
    {
        // V1 - JSON File based
        builder.Services.AddScoped<IStationService, StationService>();
    }

    private static void ConfigureV2Services(WebApplicationBuilder builder)
    {
        // V2 - CosmosDB based
        builder.Services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config["CosmosDb:ConnectionString"];

            if (string.IsNullOrEmpty(connectionString))
            {
                // Para desenvolvimento, podemos usar um connection string local ou emulator
                throw new InvalidOperationException("CosmosDB connection string is not configured");
            }

            return new CosmosClient(connectionString, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway
            });
        });

        builder.Services.AddScoped<IStationService, CosmoStationService>();
    }
}