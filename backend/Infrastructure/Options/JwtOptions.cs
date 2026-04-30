namespace Gulora.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Gulora";
    public string Audience { get; set; } = "Gulora";
    public string SigningKey { get; set; } = "";
    public int AccessTokenMinutes { get; set; } = 60;
}
