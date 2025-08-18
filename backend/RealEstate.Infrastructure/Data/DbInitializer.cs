using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RealEstate.Core.Models;

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

                // Migration 적용
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    logger.LogInformation("마이그레이션 적용 중...");
                    await context.Database.MigrateAsync();
                }
                else
                {
                    // Migration이 없는 경우에만 EnsureCreated 사용
                    await context.Database.EnsureCreatedAsync();
                }

                logger.LogInformation("데이터베이스 준비 완료");

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

                    logger.LogInformation("기본 관리자 계정이 생성되었습니다. (Email: admin@realestate.com, Password: Admin123!)");
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
