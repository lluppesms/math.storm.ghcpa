using MathStorm.Web.Components;
using MathStorm.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add OpenTelemetry and Application Insights
var appInsightsConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    // Add Application Insights for telemetry collection
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = appInsightsConnectionString;
    });
}

// Read base URL from configuration
var baseUrl = builder.Configuration.GetValue<string>("FunctionService:BaseUrl");
// Add the HttpClient instance to the service container
builder.Services.AddSingleton(new HttpClient
{
    BaseAddress = new Uri(baseUrl)
});

// Add function-based game service (calls Azure Functions for all game operations)
builder.Services.AddScoped<MathStorm.Common.Services.IGameService, MathStorm.Web.Services.GameService>();

// Add function service
builder.Services.AddScoped<IRemoteFunctionsService, RemoteFunctionsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    //The default HSTS value is 30 days.You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
