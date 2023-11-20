using FluentValidation;

namespace DineClickAPI.Validators;

public class CrupdateCityDtoValidator : AbstractValidator<CrupdateCityDto>
{
    public CrupdateCityDtoValidator()
    {
        RuleFor(c => c.Latitude).NotEmpty().InclusiveBetween(-90, 90);
        RuleFor(c => c.Longitude).NotEmpty().InclusiveBetween(-180, 180);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(30);
    }
}
