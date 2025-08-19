using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RealEstate.Core.Settings;

namespace RealEstate.API.Extensions;

public static class ApiConfigurationExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "RealEstate.API", Version = "v1" });
            // JWT 인증을 위한 Swagger UI 설정 추가
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { /* ... */ });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement { /* ... */ });
        });
        return services;
    }
}