using System.ComponentModel.DataAnnotations;

namespace RealEstate.Core.DTOs.Auth
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "사용자명은 필수입니다.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "사용자명은 3-100자 사이여야 합니다.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "이메일은 필수입니다.")]
        [EmailAddress(ErrorMessage = "올바른 이메일 형식이 아닙니다.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "비밀번호는 필수입니다.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "비밀번호는 6-100자 사이여야 합니다.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "비밀번호 확인은 필수입니다.")]
        [Compare("Password", ErrorMessage = "비밀번호가 일치하지 않습니다.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
