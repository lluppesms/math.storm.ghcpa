using MathStorm.Web.Components;
using MathStorm.Web.Services;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add game service
builder.Services.AddScoped<IGameService, GameService>();

// Add Cosmos DB services
if (builder.Environment.IsDevelopment())
{
    // Use mock service in development
    builder.Services.AddScoped<ICosmosDbService, MockCosmosDbService>();
}
else
{
    builder.Services.AddSingleton<CosmosClient>(provider =>
    {
        var configuration = provider.GetService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("CosmosDb") ?? 
                              configuration["CosmosDb:ConnectionString"];
        return new CosmosClient(connectionString);
    });

    builder.Services.AddScoped<ICosmosDbService, CosmosDbService>();
    builder.Services.AddScoped<CosmosDbInitializationService>();
}

var app = builder.Build();

// Initialize Cosmos DB (only in production)
if (!app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var cosmosDbInit = scope.ServiceProvider.GetRequiredService<CosmosDbInitializationService>();
        await cosmosDbInit.InitializeAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
