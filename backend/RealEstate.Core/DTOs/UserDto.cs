namespace RealEstate.Core.DTOs;

/// <summary>
/// 사용자 정보를 클라이언트에 전달하기 위한 데이터 전송 객체입니다.
/// </summary>
public record UserDto(
    int Id,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string Role,
    bool IsEmailVerified,
    DateTime CreatedAt
);