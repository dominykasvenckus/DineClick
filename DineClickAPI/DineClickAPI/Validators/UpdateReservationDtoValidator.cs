using FluentValidation;

namespace DineClickAPI.Validators;

public class UpdateReservationDtoValidator : AbstractValidator<UpdateReservationDto>
{
    public UpdateReservationDtoValidator()
    {
        RuleFor(r => r.Date).Must(BeValidDate).WithMessage("'Date' must be a valid date.");
        RuleFor(r => r.Time).Must(BeValidTime).WithMessage("'Time' must be a valid time.");
        RuleFor(r => r.PartySize).GreaterThan(0);
        RuleFor(r => r.Status).IsInEnum();
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
