using Microsoft.Extensions.Logging;
using RealEstate.Core.Interfaces;

namespace RealEstate.Infrastructure.Services;

/// <summary>
/// 이메일 발송 서비스의 스텁(Stub) 구현체입니다. 실제 이메일을 보내는 대신 로그를 기록합니다.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("이메일 발송 시뮬레이션:\n 수신자: {To}\n 제목: {Subject}\n 본문: {Body}", to, subject, htmlBody);
        return Task.CompletedTask;
    }
}