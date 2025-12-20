using fs_2025_assessment_1_74154.Background;
using fs_2025_assessment_1_74154.Extensions;
using fs_2025_assessment_1_74154.Services;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddCors(p => p.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

var version = builder.Configuration["ApiVersion"] ?? "V1";

if (version == "V1")
{
    builder.Services.AddSingleton<IStationService, StationService>();
}
else
{
    builder.Services.AddSingleton<CosmosClient>(provider =>
    {
        var config = provider.GetRequiredService<IConfiguration>();
        var connectionString = config["CosmosDb:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("WARNING: CosmosDB not configured. Using JSON fallback.");
            return null!; // DI will pass null to CosmoStationService (it expects CosmosClient?)
        }

        return new CosmosClient(
            connectionString,
            new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway
            });
    });


    builder.Services.AddSingleton<IStationService, CosmoStationService>();
    builder.Services.AddHostedService<StationUpdateService>();
}

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Endpoints
app.MapStationEndpoints();

app.Run();
