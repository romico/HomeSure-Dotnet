using System.ComponentModel.DataAnnotations;

namespace RealEstate.Core.Models
{
    public class PropertyImage
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }
        public Property Property { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(200)]
        public string? AltText { get; set; }

        [StringLength(500)]
        public string? Caption { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsPrimary { get; set; } = false;
        public bool IsActive { get; set; } = true;

        // 이미지 메타데이터
        public long? FileSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        [StringLength(50)]
        public string? ContentType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
