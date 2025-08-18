using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RealEstate.Core.DTOs.Auth;
using RealEstate.Core.Interfaces;
using RealEstate.Core.Models;
using RealEstate.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RealEstate.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // 입력값 검증
            if (string.IsNullOrWhiteSpace(registerDto.Email) || 
                string.IsNullOrWhiteSpace(registerDto.Username) ||
                string.IsNullOrWhiteSpace(registerDto.Password))
            {
                throw new ArgumentException("필수 입력값이 누락되었습니다.");
            }

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
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                PasswordHash = HashPassword(registerDto.Password)
                // CreatedAt, UpdatedAt은 SaveChanges에서 자동 설정됨
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.EmailOrUsername || u.Username == loginDto.EmailOrUsername);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("이메일/사용자명 또는 비밀번호가 잘못되었습니다.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("비활성화된 계정입니다.");
            }

            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            // TODO: Refresh token 로직 구현 (간단한 버전)
            throw new NotImplementedException("Refresh token 기능은 추후 구현됩니다.");
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            // TODO: Token revocation 로직 구현
            return await Task.FromResult(true);
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            return MapToUserDto(user);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            return MapToUserDto(user);
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

            var user = await _context.Users.FindAsync(userId);
            if (user == null || !VerifyPassword(currentPassword, user.PasswordHash))
            {
                return false;
            }

            user.PasswordHash = HashPassword(newPassword);
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

        private AuthResponseDto GenerateAuthResponse(User user)
        {
            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponseDto
            {
                Token = token.Token,
                Expires = token.Expires,
                RefreshToken = refreshToken,
                User = MapToUserDto(user)
            };
        }

        private (string Token, DateTime Expires) GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            // JWT 설정값 검증
            var secret = jwtSettings["Secret"];
            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("JWT Secret이 설정되지 않았습니다.");
            }

            var issuer = jwtSettings["Issuer"] ?? "RealEstateAPI";
            var audience = jwtSettings["Audience"] ?? "RealEstateUsers";

            // 만료 시간 설정 (설정에서 읽거나 기본값 7일)
            var expiryDays = int.TryParse(jwtSettings["ExpiryDays"], out var days) ? days : 7;
            var expires = DateTime.UtcNow.AddDays(expiryDays);

            var key = Encoding.UTF8.GetBytes(secret);

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

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64]; // 더 긴 토큰으로 보안 강화
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string HashPassword(string password)
        {
            // PBKDF2를 사용한 해시 생성 (BCrypt 대신)
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            // salt + hash 를 Base64로 인코딩
            var result = new byte[48]; // 16 + 32
            Array.Copy(salt, 0, result, 0, 16);
            Array.Copy(hash, 0, result, 16, 32);

            return Convert.ToBase64String(result);
        }

        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                {
                    return false;
                }

                var hashBytes = Convert.FromBase64String(hash);
                if (hashBytes.Length != 48) // 16 + 32
                {
                    return false;
                }

                // salt 추출
                var salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);

                // 저장된 해시 추출
                var storedHash = new byte[32];
                Array.Copy(hashBytes, 16, storedHash, 0, 32);

                // 입력된 비밀번호로 해시 생성
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                var testHash = pbkdf2.GetBytes(32);

                // 해시 비교
                return CryptographicOperations.FixedTimeEquals(storedHash, testHash);
            }
            catch
            {
                return false;
            }
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
