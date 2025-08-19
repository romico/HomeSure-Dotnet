using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;


namespace RealEstate.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // FluentValidation 설정
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // 도메인 서비스들 등록
        // services.AddScoped<IPropertyService, PropertyService>();
        // services.AddScoped<IUserService, UserService>();
        // services.AddScoped<ITransactionService, TransactionService>();

        return services;
    }
}
