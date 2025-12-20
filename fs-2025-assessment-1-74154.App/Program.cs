using fs_2025_assessment_1_74154.App.Components;
using fs_2025_assessment_1_74154_App;
using fs_2025_assessment_1_74154_App.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient + StationsApiClient for calling the DublinBikes API
builder.Services.AddHttpClient<IStationsApiClient, StationsApiClient>(client =>
{
    var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7146";
    client.BaseAddress = new Uri(apiBaseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
