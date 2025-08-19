using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RealEstate.Core.Entities;

namespace RealEstate.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                logger.LogInformation("데이터베이스 초기화 시작...");
                
                // 마이그레이션을 사용하여 데이터베이스 스키마를 최신 상태로 관리합니다.
                await context.Database.MigrateAsync();
                logger.LogInformation("데이터베이스 마이그레이션 완료");

                // 관리자 계정이 없으면 생성
                if (!await context.Users.AnyAsync())
                {
                    var adminUser = new User
                    {
                        Username = "admin",
                        Email = "admin@realestate.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                        FirstName = "시스템",
                        LastName = "관리자",
                        Role = "Admin",
                        IsActive = true,
                        IsEmailVerified = true
                        // CreatedAt, UpdatedAt은 SaveChanges에서 자동 설정됨
                    };

                    context.Users.Add(adminUser);
                    await context.SaveChangesAsync();

                    logger.LogInformation("기본 관리자 계정이 생성되었습니다. (Email: admin@realestate.com)");
                }

                logger.LogInformation("데이터베이스 초기화 완료");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "데이터베이스 초기화 중 오류 발생");
                throw;
            }
        }
    }
}
