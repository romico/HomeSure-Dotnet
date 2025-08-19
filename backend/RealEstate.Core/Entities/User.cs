namespace RealEstate.Core.Entities;

/// <summary>
/// 사용자 정보를 나타내는 도메인 엔티티입니다.
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;

    // DbContext 구성에 따라 추가된 속성들
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // 기본값 설정
    public string? WalletAddress { get; set; }
    
    public bool IsActive { get; set; } = true; // 기본값 설정
    public bool IsEmailVerified { get; set; } = false; // 기본값 설정

    // 생성 및 수정 타임스탬프
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}