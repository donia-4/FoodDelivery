using ApiGateway.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ApiGateway.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddGatewayAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var identitySettings = configuration
            .GetSection(IdentitySettings.SectionName)
            .Get<IdentitySettings>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{IdentitySettings.SectionName}' is missing.");

        services
            .AddOptions<IdentitySettings>()
            .Bind(configuration.GetSection(IdentitySettings.SectionName))
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;

                options.Authority = identitySettings.AuthorityUrl;

                options.Audience = identitySettings.ApiResourceName;
            });

        return services;
    }
}