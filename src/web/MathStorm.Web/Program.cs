using MathStorm.Web.Components;
using MathStorm.Web.Services;
using MathStorm.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HTTP client for function service
builder.Services.AddHttpClient<IMathStormFunctionService, MathStormFunctionService>(client =>
{
    // For development, point to local function app
    client.BaseAddress = new Uri("http://localhost:7071");
});

// Add game service (use shared implementation)
builder.Services.AddScoped<MathStorm.Shared.Services.IGameService, MathStorm.Shared.Services.GameService>();

// Add function service
builder.Services.AddScoped<IMathStormFunctionService, MathStormFunctionService>();

// Add mock cosmos service for development (functions handle real DB access)
builder.Services.AddScoped<MathStorm.Shared.Services.ICosmosDbService, MathStorm.Shared.Services.MockCosmosDbService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
//}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
