namespace RealEstate.Core.Interfaces;

/// <summary>
/// 이메일 발송을 위한 서비스 인터페이스입니다.
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}