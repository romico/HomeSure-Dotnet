namespace RealEstate.Core.Entities;

/// <summary>
/// 사용자 인증 갱신을 위한 리프레시 토큰 엔티티입니다.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime Expires { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
    public User User { get; set; } = null!;
    public string RevokedBy { get; set; }
}