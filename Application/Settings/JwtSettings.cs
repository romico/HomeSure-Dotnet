namespace WorkerService1.Application.Settings;

/// <summary>
/// appsettings.json의 JWT 설정값을 담는 클래스입니다.
/// IOptions<JwtSettings> 패턴을 통해 주입됩니다.
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationHours { get; set; }
}