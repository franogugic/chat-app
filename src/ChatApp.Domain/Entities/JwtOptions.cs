namespace ChatApp.Domain.Entities;

public class JwtOptions
{
    // Ime konfiguracije u appsettings.json
    public const string SectionName = "Jwt";
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    
    public int ExpireMinutes { get; set; }
    
    public int RefreshTokenExpireDays { get; set; }
}