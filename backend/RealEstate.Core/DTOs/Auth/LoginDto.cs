using System.ComponentModel.DataAnnotations;

namespace RealEstate.Core.DTOs.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage = "이메일 또는 사용자명은 필수입니다.")]
        public string EmailOrUsername { get; set; } = string.Empty;

        [Required(ErrorMessage = "비밀번호는 필수입니다.")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }
}
