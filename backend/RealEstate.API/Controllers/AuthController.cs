using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Core.DTOs.Auth;
using RealEstate.Core.Interfaces;
using System.Security.Claims;

namespace RealEstate.API.Controllers
{
    /// <summary>
    /// 사용자 인증 관리 컨트롤러
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// 사용자 회원가입
        /// </summary>
        /// <param name="registerDto">회원가입 정보</param>
        /// <returns>인증 토큰 및 사용자 정보</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation("회원가입 시도: {Email}", registerDto.Email);

                var result = await _userService.RegisterAsync(registerDto);

                _logger.LogInformation("회원가입 성공: {UserId}", result.User.Id);

                return Ok(new
                {
                    success = true,
                    message = "회원가입이 완료되었습니다.",
                    data = result
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("회원가입 실패: {Message}", ex.Message);
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "회원가입 중 오류 발생");
                return StatusCode(500, new
                {
                    success = false,
                    message = "회원가입 중 오류가 발생했습니다."
                });
            }
        }

        /// <summary>
        /// 사용자 로그인
        /// </summary>
        /// <param name="loginDto">로그인 정보</param>
        /// <returns>인증 토큰 및 사용자 정보</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("로그인 시도: {EmailOrUsername}", loginDto.EmailOrUsername);

                var result = await _userService.LoginAsync(loginDto);

                _logger.LogInformation("로그인 성공: {UserId}", result.User.Id);

                return Ok(new
                {
                    success = true,
                    message = "로그인에 성공했습니다.",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("로그인 실패: {Message}", ex.Message);
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "로그인 중 오류 발생");
                return StatusCode(500, new
                {
                    success = false,
                    message = "로그인 중 오류가 발생했습니다."
                });
            }
        }

        /// <summary>
        /// 토큰 갱신
        /// </summary>
        /// <param name="request">리프레시 토큰 요청</param>
        /// <returns>새로운 인증 토큰</returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _userService.RefreshTokenAsync(request.RefreshToken);
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "토큰 갱신 중 오류 발생");
                return Unauthorized(new
                {
                    success = false,
                    message = "유효하지 않은 토큰입니다."
                });
            }
        }

        /// <summary>
        /// 로그아웃
        /// </summary>
        /// <returns>로그아웃 결과</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Split(" ").Last();

                if (!string.IsNullOrEmpty(token))
                {
                    await _userService.RevokeTokenAsync(token);
                }

                _logger.LogInformation("사용자 로그아웃: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                return Ok(new
                {
                    success = true,
                    message = "로그아웃되었습니다."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "로그아웃 중 오류 발생");
                return StatusCode(500, new
                {
                    success = false,
                    message = "로그아웃 중 오류가 발생했습니다."
                });
            }
        }

        /// <summary>
        /// 현재 사용자 정보 조회
        /// </summary>
        /// <returns>사용자 정보</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "사용자를 찾을 수 없습니다."
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "사용자 정보 조회 중 오류 발생");
                return StatusCode(500, new
                {
                    success = false,
                    message = "사용자 정보를 가져오는 중 오류가 발생했습니다."
                });
            }
        }

        /// <summary>
        /// 이메일 인증
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="token">인증 토큰</param>
        /// <returns>인증 결과</returns>
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] int userId, [FromQuery] string token)
        {
            try
            {
                var result = await _userService.VerifyEmailAsync(userId, token);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "이메일 인증이 완료되었습니다."
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    message = "유효하지 않은 인증 토큰입니다."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "이메일 인증 중 오류 발생");
                return StatusCode(500, new
                {
                    success = false,
                    message = "이메일 인증 중 오류가 발생했습니다."
                });
            }
        }
    }
}
