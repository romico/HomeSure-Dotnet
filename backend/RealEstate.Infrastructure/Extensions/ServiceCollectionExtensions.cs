using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealEstate.Core.Settings;
using RealEstate.Core.Interfaces;
using RealEstate.Infrastructure.Data.Repositories;
using RealEstate.Infrastructure.Services;
using ITransactionRepository = RealEstate.Core.Interfaces.ITransactionRepository;

namespace RealEstate.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 강력한 형식의 설정 등록
        services.Configure<BlockchainSettings>(configuration.GetSection("BlockchainSettings"));

        // 리포지토리 패턴 등록
        services.AddScoped<IPropertyRepository, PropertyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // 외부 서비스 등록
        services.AddScoped<IBlockchainService, BlockchainService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        // HTTP 클라이언트 설정
        services.AddHttpClient();

        return services;
    }
}
