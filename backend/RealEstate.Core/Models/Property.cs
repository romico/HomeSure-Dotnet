using System.ComponentModel.DataAnnotations;

namespace RealEstate.Core.Models
{
    public class Property
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? ZipCode { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public int? Bedrooms { get; set; }
        public int? Bathrooms { get; set; }
        public int? SquareFeet { get; set; }
        public int? LotSize { get; set; }

        [StringLength(50)]
        public string PropertyType { get; set; } = "House"; // House, Apartment, Condo, Commercial, etc.

        [StringLength(50)]
        public string Status { get; set; } = "Available"; // Available, Sold, Rented, UnderContract

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        // 블록체인 관련
        public string? TokenId { get; set; }
        public string? ContractAddress { get; set; }
        public bool IsTokenized { get; set; } = false;

        // 관계형 속성
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
