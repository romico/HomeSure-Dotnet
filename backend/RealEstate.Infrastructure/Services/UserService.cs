using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RealEstate.Core.DTOs;
using RealEstate.Core.DTOs.Auth;
using RealEstate.Core.Entities; // Models 대신 Entities를 사용합니다.
using RealEstate.Core.Settings;
using RealEstate.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using RealEstate.Core.Interfaces;

namespace RealEstate.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;

        public UserService(ApplicationDbContext context, IOptions<JwtSettings> jwtSettings, IMapper mapper)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // 유효성 검사는 API 계층의 FluentValidation 미들웨어에서 처리됩니다.
            // 이메일 중복 확인
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                throw new ArgumentException("이미 사용 중인 이메일입니다.");
            }

            // 사용자명 중복 확인
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                throw new ArgumentException("이미 사용 중인 사용자명입니다.");
            }

            // 사용자 생성
            var user = new Core.Entities.User // 명시적으로 엔티티 타입을 사용합니다.
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
                // CreatedAt, UpdatedAt은 SaveChanges에서 자동 설정됨
            };

            _context.Users.Add(user); // 이제 user는 올바른 타입인 Entities.User 입니다.
            await _context.SaveChangesAsync();

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.EmailOrUsername || u.Username == loginDto.EmailOrUsername);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("이메일/사용자명 또는 비밀번호가 잘못되었습니다.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("비활성화된 계정입니다.");
            }

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (storedToken == null)
            {
                throw new SecurityTokenException("유효하지 않은 리프레시 토큰입니다.");
            }

            if (storedToken.IsRevoked || storedToken.Expires < DateTime.UtcNow)
            {
                throw new SecurityTokenException("만료되었거나 폐기된 리프레시 토큰입니다.");
            }

            // 여러 DB 업데이트를 하나의 트랜잭션으로 묶어 원자성을 보장합니다.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // 기존 리프레시 토큰을 폐기합니다 (선택적: 토큰 순환 전략).
            storedToken.IsRevoked = true;
            _context.Update(storedToken);

            // 새로운 토큰 세트를 발급합니다.
            var authResponse = await GenerateAuthResponse(storedToken.User);
            
            await transaction.CommitAsync();
            return authResponse;
        }

        public Task<bool> RevokeTokenAsync(string token)
        {
            // TODO: Token revocation 로직 구현
            return Task.FromResult(true); // async/await가 불필요하므로 Task.FromResult를 직접 반환합니다.
        }

        public async Task<Core.DTOs.UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            return _mapper.Map<UserDto>(user);
        }

        public async Task<Core.DTOs.UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> VerifyEmailAsync(int userId, string token)
        {
            // TODO: 이메일 인증 로직 구현
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsEmailVerified = true;
                // UpdatedAt은 SaveChanges에서 자동 설정됨
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                return false;
            }

            var user = await _context.Users.FindAsync(userId); // ChangePassword는 추적이 필요하므로 FindAsync 사용
            if (user == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            // UpdatedAt은 SaveChanges에서 자동 설정됨
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            // TODO: 비밀번호 재설정 로직 구현
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user != null;
        }

        private async Task<AuthResponseDto> GenerateAuthResponse(Core.Entities.User user)
        {
            var (token, expires) = GenerateJwtToken(user);
            var refreshTokenString = GenerateRefreshTokenString();

            var refreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(7), // 리프레시 토큰 만료 기간 설정
                CreatedAt = DateTime.UtcNow
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            // record 생성자를 사용하여 DTO를 생성합니다.
            return new AuthResponseDto(token, refreshTokenString, expires, _mapper.Map<UserDto>(user));
        }

        private (string Token, DateTime Expires) GenerateJwtToken(Core.Entities.User user)
        {
            // IOptions<JwtSettings>를 사용하여 강력한 형식의 설정에 접근합니다.
            if (string.IsNullOrEmpty(_jwtSettings.SecretKey))
            {
                throw new InvalidOperationException("JWT SecretKey가 설정되지 않았습니다.");
            }

            var issuer = _jwtSettings.Issuer;
            var audience = _jwtSettings.Audience;
            var expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours);

            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, 
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                        ClaimValueTypes.Integer64)
                }),
                Expires = expires,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return (tokenHandler.WriteToken(token), expires);
        }

        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64]; // 더 긴 토큰으로 보안 강화
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
