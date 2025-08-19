namespace RealEstate.Core.Entities;

/// <summary>
/// 부동산 매물의 이미지 정보를 나타내는 엔티티입니다.
/// </summary>
public class PropertyImage
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
    public long? FileSize { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? ContentType { get; set; }
    public DateTime CreatedAt { get; set; }
    public Property Property { get; set; } = null!;
}