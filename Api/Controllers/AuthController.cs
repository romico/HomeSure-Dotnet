using Microsoft.AspNetCore.Mvc;
using WorkerService1.Api.Contracts;
using WorkerService1.Application.Interfaces;

namespace WorkerService1.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    // 생성자 주입을 통해 인증 서비스에 대한 의존성을 받습니다.
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        // 인증 서비스에 토큰 생성을 위임합니다.
        var token = await _authService.AuthenticateAsync(request.Username, request.Password, cancellationToken);

        // 토큰 생성에 실패하면(자격 증명 실패) 401 Unauthorized를 반환합니다.
        return string.IsNullOrEmpty(token) ? Unauthorized() : Ok(new LoginResponse(token));
    }
}