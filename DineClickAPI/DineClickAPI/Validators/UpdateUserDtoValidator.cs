using FluentValidation;

namespace DineClickAPI.Validators;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(u => u.Email).EmailAddress();
        RuleFor(u => u.FirstName).NotEmpty().MaximumLength(30);
        RuleFor(u => u.LastName).NotEmpty().MaximumLength(30);
    }
}
