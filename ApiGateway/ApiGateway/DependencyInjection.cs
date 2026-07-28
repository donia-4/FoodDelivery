using System.Threading.RateLimiting;
using ApiGateway.Extensions;
using ApiGateway.Middlewares;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;

namespace ApiGateway;

public static class DependencyInjection
{
    public static IServiceCollection AddGatewayServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddGatewayAuthentication(configuration)   
            .AddGatewayAuthorization()
            .AddGatewayCors(configuration)
            .AddGatewayOutputCaching()
            .AddGatewayHealthChecks()
            .AddGatewayExceptionHandling()
            .AddGatewayProblemDetails()
            .AddGatewayRateLimiting()
            .AddGatewayOcelot(configuration);

        return services;
    }

    private static IServiceCollection AddGatewayCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCorsPolicy", builder =>
            {
                var allowedOrigins =
                    configuration
                        .GetSection("Cors:AllowedOrigins")
                        .Get<string[]>() ?? [];

                if (allowedOrigins.Length > 0)
                {
                    builder.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
                else
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        return services;
    }

    private static IServiceCollection AddGatewayOutputCaching(
        this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            options.AddPolicy(
                "DefaultCache",
                policy => policy.Expire(TimeSpan.FromMinutes(1)));
        });

        return services;
    }

    private static IServiceCollection AddGatewayHealthChecks(
        this IServiceCollection services)
    {
        services.AddHealthChecks();

        return services;
    }

    private static IServiceCollection AddGatewayExceptionHandling(
        this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    private static IServiceCollection AddGatewayProblemDetails(
        this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                context.ProblemDetails.Extensions.Add(
                    "requestId",
                    context.HttpContext.TraceIdentifier);
            };
        });

        return services;
    }

    private static IServiceCollection AddGatewayRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddSlidingWindowLimiter(
                "SlidingWindow",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 100;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.SegmentsPerWindow = 6;
                    limiterOptions.QueueLimit = 10;
                    limiterOptions.QueueProcessingOrder =
                        QueueProcessingOrder.OldestFirst;
                    limiterOptions.AutoReplenishment = true;
                });

            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;
        });

        return services;
    }

    private static IServiceCollection AddGatewayOcelot(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOcelot(configuration);
        services.AddSwaggerForOcelot(configuration);
        return services;
    }
}