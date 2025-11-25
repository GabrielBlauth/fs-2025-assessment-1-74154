using Asp.Versioning;
using fs_2025_assessment_1_74154.Models;
using fs_2025_assessment_1_74154.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1.0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Station Service
builder.Services.AddSingleton<IStationService, StationService>();

// Add Background Service
builder.Services.AddHostedService<StationUpdateBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Versioned Minimal APIs
var v1 = app.NewApiVersionSet("v1").HasApiVersion(new ApiVersion(1.0)).Build();
var v2 = app.NewApiVersionSet("v2").HasApiVersion(new ApiVersion(2.0)).Build();

// V1 - File-based JSON
app.MapGet("/api/stations", (
    IStationService stationService,
    string? status,
    int? minBikes,
    string? search,
    string sort = "name",
    string dir = "asc",
    int page = 1,
    int pageSize = 20)
    => { /* Your existing V1 implementation */ })
    .WithApiVersionSet(v1)
    .MapToApiVersion(1.0);

app.MapGet("/api/stations/{number}", (IStationService stationService, int number) => { /* Your existing implementation */ })
    .WithApiVersionSet(v1)
    .MapToApiVersion(1.0);

app.MapGet("/api/stations/summary", (IStationService stationService) => { /* Your existing implementation */ })
    .WithApiVersionSet(v1)
    .MapToApiVersion(1.0);

app.MapPost("/api/stations", (IStationService stationService, Station station) => { /* Your existing implementation */ })
    .WithApiVersionSet(v1)
    .MapToApiVersion(1.0);

app.MapPut("/api/stations/{number}", (IStationService stationService, int number, Station station) => { /* Your existing implementation */ })
    .WithApiVersionSet(v1)
    .MapToApiVersion(1.0);

// V2 - CosmosDB (Placeholder - same as V1 for now)
app.MapGet("/v{version:apiVersion}/stations", (
    IStationService stationService,
    string? status,
    int? minBikes,
    string? search,
    string sort = "name",
    string dir = "asc",
    int page = 1,
    int pageSize = 20)
    => { /* Same as V1 for now */ })
    .WithApiVersionSet(v2)
    .MapToApiVersion(2.0);

app.MapGet("/v{version:apiVersion}/stations/{number}", (IStationService stationService, int number) => { /* Same as V1 */ })
    .WithApiVersionSet(v2)
    .MapToApiVersion(2.0);

app.MapGet("/v{version:apiVersion}/stations/summary", (IStationService stationService) => { /* Same as V1 */ })
    .WithApiVersionSet(v2)
    .MapToApiVersion(2.0);

app.MapPost("/v{version:apiVersion}/stations", (IStationService stationService, Station station) => { /* Same as V1 */ })
    .WithApiVersionSet(v2)
    .MapToApiVersion(2.0);

app.MapPut("/v{version:apiVersion}/stations/{number}", (IStationService stationService, int number, Station station) => { /* Same as V1 */ })
    .WithApiVersionSet(v2)
    .MapToApiVersion(2.0);

app.Run();