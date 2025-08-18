using Microsoft.AspNetCore.Mvc;

namespace RealEstate.API.Controllers
{
    /// <summary>
    /// 부동산 매물 관리 API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PropertiesController : ControllerBase
    {
        /// <summary>
        /// 모든 매물 조회
        /// </summary>
        /// <returns>매물 목록</returns>
        /// <response code="200">매물 목록 조회 성공</response>
        [HttpGet]
        [ProducesResponseType(200)]
        public IActionResult GetProperties()
        {
            var properties = new[]
            {
                new
                {
                    id = 1,
                    title = "서울 강남구 아파트",
                    price = 1500000000,
                    area = 84.5,
                    location = "서울시 강남구",
                    type = "아파트"
                },
                new
                {
                    id = 2,
                    title = "부산 해운대 오피스텔",
                    price = 350000000,
                    area = 33.2,
                    location = "부산시 해운대구",
                    type = "오피스텔"
                }
            };

            return Ok(new
            {
                success = true,
                data = properties,
                count = properties.Length
            });
        }

        /// <summary>
        /// 특정 매물 조회
        /// </summary>
        /// <param name="id">매물 ID</param>
        /// <returns>매물 상세 정보</returns>
        /// <response code="200">매물 조회 성공</response>
        /// <response code="404">매물을 찾을 수 없음</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult GetProperty(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid property ID");

            var property = new
            {
                id,
                title = "서울 강남구 아파트",
                price = 1500000000,
                area = 84.5,
                location = "서울시 강남구 테헤란로 123",
                type = "아파트",
                description = "교통이 편리한 강남구 아파트입니다.",
                bedrooms = 3,
                bathrooms = 2,
                createdAt = DateTime.UtcNow.AddDays(-10)
            };

            return Ok(new
            {
                success = true,
                data = property
            });
        }

        /// <summary>
        /// 새 매물 등록
        /// </summary>
        /// <param name="request">매물 등록 정보</param>
        /// <returns>등록된 매물 정보</returns>
        /// <response code="201">매물 등록 성공</response>
        /// <response code="400">잘못된 요청</response>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public IActionResult CreateProperty([FromBody] CreatePropertyRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required");

            var newProperty = new
            {
                id = Random.Shared.Next(1000, 9999),
                title = request.Title,
                price = request.Price,
                area = request.Area,
                location = request.Location,
                type = request.Type,
                createdAt = DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetProperty), new { id = newProperty.id }, new
            {
                success = true,
                message = "매물이 성공적으로 등록되었습니다.",
                data = newProperty
            });
        }

        /// <summary>
        /// 매물 정보 수정
        /// </summary>
        /// <param name="id">매물 ID</param>
        /// <param name="request">수정할 매물 정보</param>
        /// <returns>수정된 매물 정보</returns>
        /// <response code="200">매물 수정 성공</response>
        /// <response code="404">매물을 찾을 수 없음</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult UpdateProperty(int id, [FromBody] CreatePropertyRequest request)
        {
            if (id <= 0)
                return BadRequest("Invalid property ID");

            return Ok(new
            {
                success = true,
                message = "매물 정보가 수정되었습니다.",
                data = new { id, updatedAt = DateTime.UtcNow }
            });
        }

        /// <summary>
        /// 매물 삭제
        /// </summary>
        /// <param name="id">매물 ID</param>
        /// <returns>삭제 결과</returns>
        /// <response code="200">매물 삭제 성공</response>
        /// <response code="404">매물을 찾을 수 없음</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult DeleteProperty(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid property ID");

            return Ok(new
            {
                success = true,
                message = "매물이 삭제되었습니다."
            });
        }
    }

    /// <summary>
    /// 매물 등록 요청 모델
    /// </summary>
    public class CreatePropertyRequest
    {
        /// <summary>매물 제목</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>매물 가격 (원)</summary>
        public long Price { get; set; }

        /// <summary>면적 (㎡)</summary>
        public double Area { get; set; }

        /// <summary>위치</summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>매물 유형 (아파트, 오피스텔, 빌라 등)</summary>
        public string Type { get; set; } = string.Empty;
    }
}
