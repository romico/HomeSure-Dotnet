using Microsoft.AspNetCore.Mvc;

namespace RealEstate.API.Controllers
{
    /// <summary>
    /// 기본 API 정보 컨트롤러
    /// </summary>
    [ApiController]
    [Route("api")]
    public class HomeController : ControllerBase
    {
        /// <summary>
        /// 기본 API 정보
        /// </summary>
        /// <returns>API 정보 JSON</returns>
        [HttpGet("/")]
        public IActionResult Index()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            return Ok(new
            {
                name = "RealEstate DApp API",
                description = "부동산 거래 플랫폼 API",
                version = "1.0.0",
                status = "Running",
                timestamp = DateTime.UtcNow,
                links = new
                {
                    documentation = $"{baseUrl}/swagger",
                    status = $"{baseUrl}/api/status"
                },
                endpoints = new
                {
                    swagger = "/swagger",
                    status = "/api/status"
                }
            });
        }

        /// <summary>
        /// API 상태 확인
        /// </summary>
        /// <returns>API 상태 정보</returns>
        [HttpGet("/api/status")]
        public IActionResult Status()
        {
            return Ok(new
            {
                status = "Running",
                message = "RealEstate DApp API is running successfully",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
