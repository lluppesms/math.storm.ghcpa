using MathStorm.Web.Components;
using MathStorm.Web.Services;
using MathStorm.Services;

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

// Add MathStorm services (direct calls instead of Azure Functions HTTP calls)
builder.Services.AddMathStormServices(builder.Configuration);

// Add game service that uses the local MathStorm service
builder.Services.AddScoped<MathStorm.Common.Services.IGameService, MathStorm.Web.Services.GameService>();

// Add user profile service
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

// Add build info service
builder.Services.AddScoped<IBuildInfoService, BuildInfoService>();

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
