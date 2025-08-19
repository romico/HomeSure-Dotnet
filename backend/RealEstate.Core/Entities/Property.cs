namespace RealEstate.Core.Entities;

/// <summary>
/// 부동산 매물 정보를 나타내는 도메인 엔티티입니다.
/// </summary>
public class Property
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? SquareFeet { get; set; }
    public int? LotSize { get; set; }
    public string PropertyType { get; set; } = "House";
    public string Status { get; set; } = "Available";
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public string? TokenId { get; set; }
    public string? ContractAddress { get; set; }
    public bool IsTokenized { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // 관계
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
}