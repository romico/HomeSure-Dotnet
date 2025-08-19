using FluentValidation;
using RealEstate.Core.DTOs.Auth;

namespace RealEstate.Core.Validators;

/// <summary>
/// RegisterDto의 유효성을 검사합니다.
/// </summary>
public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("사용자명은 필수입니다.")
            .MinimumLength(3).WithMessage("사용자명은 3자 이상이어야 합니다.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("이메일은 필수입니다.")
            .EmailAddress().WithMessage("유효한 이메일 형식이 아닙니다.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("비밀번호는 필수입니다.")
            .MinimumLength(8).WithMessage("비밀번호는 8자 이상이어야 합니다.");
    }
}