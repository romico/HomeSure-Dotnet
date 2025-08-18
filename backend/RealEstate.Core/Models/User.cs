using System.ComponentModel.DataAnnotations;

namespace RealEstate.Core.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsEmailVerified { get; set; } = false;

        // 역할 관리
        public string Role { get; set; } = "User"; // User, Admin, Agent

        // 블록체인 지갑 주소 (향후 확장용)
        public string? WalletAddress { get; set; }
    }
}
