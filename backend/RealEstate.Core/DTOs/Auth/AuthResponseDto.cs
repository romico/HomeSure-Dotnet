using RealEstate.Core.DTOs;

namespace RealEstate.Core.DTOs.Auth
{
    /// <summary>
    /// 인증 성공 시 클라이언트에 반환되는 데이터 전송 객체입니다.
    /// </summary>
    public record AuthResponseDto(
        string Token,
        string RefreshToken,
        DateTime Expires,
        UserDto User
    );
}
