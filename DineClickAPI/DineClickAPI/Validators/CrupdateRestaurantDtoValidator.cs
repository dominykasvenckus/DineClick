using FluentValidation;

namespace DineClickAPI.Validators;

public class CrupdateRestaurantDtoValidator : AbstractValidator<CrupdateRestaurantDto>
{
    public CrupdateRestaurantDtoValidator()
    {
        RuleFor(r => r.Name).NotEmpty().MaximumLength(30);
        RuleFor(r => r.Description).NotEmpty().MaximumLength(300);
        RuleFor(r => r.StreetAddress).NotEmpty().MaximumLength(70);
        RuleFor(r => r.WebsiteUrl).Matches(@"https?:\/\/(?:www\.)?[a-zA-Z0-9]{2,}(?:\.[a-zA-Z0-9]{2,})(?:\.[a-zA-Z0-9]{2,})?");
    }
}
