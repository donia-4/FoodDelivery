namespace ApiGateway.Settings;

public sealed class IdentitySettings
{
    public const string SectionName = "Identity";

    public string AuthorityUrl { get; set; } = string.Empty;

    public string ApiResourceName { get; set; } = string.Empty;
}