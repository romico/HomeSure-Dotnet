using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WorkerService1.Application.Interfaces;
using WorkerService1.Application.Settings;

namespace WorkerService1.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;

    // IConfiguration 대신 IUserRepository와 IOptions<JwtSettings>를 주입받습니다.
    public AuthService(IUserRepository userRepository, IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<string?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken)
    {
        // 1. 데이터베이스에서 사용자 정보를 조회합니다.
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);

        if (user is null)
        {
            // 사용자가 존재하지 않으면 실패 처리합니다.
            return null;
        }

        // 2. 제공된 비밀번호가 저장된 해시와 일치하는지 확인합니다.
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        if (!isPasswordValid)
        {
            // 비밀번호가 일치하지 않으면 실패 처리합니다.
            return null;
        }

        // 3. 비밀번호가 유효하면 JWT를 생성합니다.
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // 토큰에 담을 클레임(사용자 정보)을 정의합니다.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // 주체는 보통 고유 식별자인 ID를 사용합니다.
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
            signingCredentials: credentials);

        return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}