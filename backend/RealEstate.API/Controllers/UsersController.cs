using Microsoft.AspNetCore.Mvc;

namespace RealEstate.API.Controllers
{
    /// <summary>
    /// 사용자 관리 API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// 사용자 등록
        /// </summary>
        /// <param name="request">사용자 등록 정보</param>
        /// <returns>등록 결과</returns>
        /// <response code="201">사용자 등록 성공</response>
        /// <response code="400">잘못된 요청</response>
        [HttpPost("register")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required");

            return CreatedAtAction(nameof(GetProfile), new { id = Random.Shared.Next(1000, 9999) }, new
            {
                success = true,
                message = "사용자가 성공적으로 등록되었습니다.",
                data = new
                {
                    id = Random.Shared.Next(1000, 9999),
                    email = request.Email,
                    name = request.Name,
                    createdAt = DateTime.UtcNow
                }
            });
        }

        /// <summary>
        /// 사용자 로그인
        /// </summary>
        /// <param name="request">로그인 정보</param>
        /// <returns>로그인 결과 및 토큰</returns>
        /// <response code="200">로그인 성공</response>
        /// <response code="401">인증 실패</response>
        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required");

            // 실제로는 사용자 인증 로직이 여기에 들어갑니다
            return Ok(new
            {
                success = true,
                message = "로그인 성공",
                data = new
                {
                    token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                    expiresIn = 24 * 60 * 60, // 24시간
                    user = new
                    {
                        id = 1001,
                        email = request.Email,
                        name = "사용자"
                    }
                }
            });
        }

        /// <summary>
        /// 사용자 프로필 조회
        /// </summary>
        /// <param name="id">사용자 ID</param>
        /// <returns>사용자 프로필</returns>
        /// <response code="200">프로필 조회 성공</response>
        /// <response code="404">사용자를 찾을 수 없음</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult GetProfile(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid user ID");

            return Ok(new
            {
                success = true,
                data = new
                {
                    id,
                    email = "user@example.com",
                    name = "홍길동",
                    walletAddress = "0x742d35Cc6435C4532D5B9B6C7835F7A3f5F4C7D2",
                    createdAt = DateTime.UtcNow.AddDays(-30)
                }
            });
        }
    }

    /// <summary>
    /// 사용자 등록 요청 모델
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>이메일</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>비밀번호</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>이름</summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// 로그인 요청 모델
    /// </summary>
    public class LoginRequest
    {
        /// <summary>이메일</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>비밀번호</summary>
        public string Password { get; set; } = string.Empty;
    }
}
