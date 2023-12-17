using FluentValidation;

namespace DineClickAPI.Validators;

public class CreateReservationDtoValidator : AbstractValidator<CreateReservationDto>
{
    public CreateReservationDtoValidator()
    {
        RuleFor(r => r.Date).Must(BeValidDate).WithMessage("'Date' must be a valid date.");
        RuleFor(r => r.Time).Must(BeValidTime).WithMessage("'Time' must be a valid time.");
        RuleFor(r => r.PartySize).GreaterThan(0);
    }

    private bool BeValidDate(DateOnly date)
    {
        return !date.Equals(default);
    }

    private bool BeValidTime(TimeOnly time)
    {
        return !time.Equals(default);
    }
}
