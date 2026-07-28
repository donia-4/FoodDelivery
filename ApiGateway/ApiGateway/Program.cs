using ApiGateway;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Load optional local development config
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile(
        "appsettings.Development.Local.json",
        optional: true,
        reloadOnChange: true);
}

// Load Ocelot + Swagger configuration
builder.Configuration
    .AddJsonFile(
        $"Configuration/ocelot.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true)
    .AddJsonFile(
        "Configuration/swagger.Endpoints.json",
        optional: false,
        reloadOnChange: true);

// Serilog
builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration));

// Presentation Services
builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

app.UseSwaggerForOcelotUI(options =>
{
    options.PathToSwaggerGenerator = "/swagger/docs";
});

app.UseExceptionHandler();

app.UseCors("DefaultCorsPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.UseOutputCache();

app.MapHealthChecks("/health");

// Ocelot MUST be last middleware
await app.UseOcelot();

app.Run();