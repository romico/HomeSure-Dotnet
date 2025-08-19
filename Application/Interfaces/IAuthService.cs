namespace WorkerService1.Application.Interfaces;

/// <summary>
/// 사용자 인증 로직을 처리하는 서비스 인터페이스입니다.
/// </summary>
public interface IAuthService
{
    Task<string?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken);
}