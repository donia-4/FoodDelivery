namespace ApiGateway.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string[] Audience { get; set; } = [];

    public int AccessTokenExpirationMinutes { get; set; }
}