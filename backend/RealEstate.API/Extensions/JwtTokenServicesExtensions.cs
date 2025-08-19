using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RealEstate.Core.Settings;

namespace RealEstate.API.Extensions;

/// <summary>
/// JWT 인증 관련 서비스 등록을 위한 확장 메서드 클래스입니다.
/// </summary>
public static class JwtTokenServicesExtensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // IOptions 패턴을 사용하여 JwtSettings를 DI 컨테이너에 등록합니다.
        var jwtSettingsSection = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSettingsSection);

        var jwtSettings = jwtSettingsSection.Get<JwtSettings>() 
                          ?? throw new InvalidOperationException("JwtSettings가 설정 파일에 존재하지 않습니다.");

        // 시크릿 키 유효성 검사를 강화합니다.
        if (string.IsNullOrEmpty(jwtSettings.SecretKey) || jwtSettings.SecretKey.Length < 32)
        {
            throw new InvalidOperationException("JWT SecretKey는 32자 이상이어야 합니다.");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // 개발 환경에서는 false, 프로덕션에서는 true 권장
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidateIssuer = true, // 발급자 검증 활성화
                ValidateAudience = true, // 수신자 검증 활성화
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                ClockSkew = TimeSpan.Zero // 시간 오차를 허용하지 않음
            };
        });

        return services;
    }
}