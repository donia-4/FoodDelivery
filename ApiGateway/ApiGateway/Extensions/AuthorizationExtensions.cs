namespace ApiGateway.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddGatewayAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy("UserOnly", policy =>
                policy.RequireRole("User", "Admin"));

            options.AddPolicy("Authenticated", policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }
}