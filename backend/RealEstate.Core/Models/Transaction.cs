using System.ComponentModel.DataAnnotations;

namespace RealEstate.Core.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }
        public Property Property { get; set; } = null!;

        [Required]
        public int BuyerId { get; set; }
        public User Buyer { get; set; } = null!;

        [Required]
        public int SellerId { get; set; }
        public User Seller { get; set; } = null!;

        [Required]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string TransactionType { get; set; } = "Sale"; // Sale, Rent, Lease

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled, Failed

        // 블록체인 관련
        public string? BlockchainTxHash { get; set; }
        public string? ContractAddress { get; set; }
        public bool IsOnChain { get; set; } = false;
        public int? BlockNumber { get; set; }
        public DateTime? BlockchainConfirmedAt { get; set; }

        // 추가 정보
        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
