using FluentValidation;

namespace DineClickAPI.Validators
{
    public class AddressDtoValidator : AbstractValidator<AddressDto>
    {
        public AddressDtoValidator()
        {
            RuleFor(a => a.Street).NotEmpty().MaximumLength(50);
            RuleFor(a => a.HouseNumber).NotEmpty().MaximumLength(20);
            RuleFor(a => a.City).NotEmpty().MaximumLength(30);
        }
    }
}
