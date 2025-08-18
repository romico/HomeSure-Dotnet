using System.ComponentModel.DataAnnotations;

namespace RealEstate.Core.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "리프레시 토큰은 필수입니다.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
