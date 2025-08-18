using System.ComponentModel.DataAnnotations;

namespace RealEstate.Core.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        public int UserId { get; set; }

        public DateTime Expires { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; } = false;
        public string? RevokedBy { get; set; }
        public DateTime? RevokedAt { get; set; }

        // Navigation Property
        public User User { get; set; } = null!;

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
