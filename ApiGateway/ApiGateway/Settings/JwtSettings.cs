using System.ComponentModel.DataAnnotations;

namespace ApiGateway.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; set; } = 15;
}