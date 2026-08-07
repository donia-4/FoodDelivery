using Restaurant.API;
using Restaurant.Application;
using Restaurant.Infrastructure;
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

builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers();

builder.Services.AddApplication();

builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddPresentationServices(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler();

app.UseCors("DefaultCorsPolicy");

app.UseRateLimiter();

app.UseOutputCache();

app.MapHealthChecks("/health");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();