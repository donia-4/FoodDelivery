using ApiGateway;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Load optional local development config (highest priority in Development)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile(
        "appsettings.Development.Local.json",
        optional: true,
        reloadOnChange: true);
}

// Load Ocelot configuration
builder.Configuration
    .AddJsonFile("Configuration/ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile(
        $"Configuration/ocelot.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true);

// Serilog
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration));

// Presentation Services
builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();

app.UseCors("DefaultCorsPolicy");

app.UseRateLimiter();

app.UseOutputCache();

app.MapHealthChecks("/health");

app.MapGet("/", () => "Gateway Running");

// Ocelot MUST be last middleware
await app.UseOcelot();

app.Run();