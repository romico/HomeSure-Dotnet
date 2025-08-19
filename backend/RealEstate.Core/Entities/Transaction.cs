namespace RealEstate.Core.Entities;

/// <summary>
/// 부동산 거래 정보를 나타내는 도메인 엔티티입니다.
/// </summary>
public class Transaction
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int BuyerId { get; set; }
    public int SellerId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = "Sale";
    public string Status { get; set; } = "Pending";
    public string? BlockchainTxHash { get; set; }
    public string? ContractAddress { get; set; }
    public bool IsOnChain { get; set; }
    public int? BlockNumber { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? BlockchainConfirmedAt { get; set; }

    // 탐색 속성
    public Property Property { get; set; } = null!;
    public User Buyer { get; set; } = null!;
    public User Seller { get; set; } = null!;
}