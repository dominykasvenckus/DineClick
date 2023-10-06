using FluentValidation;

namespace DineClickAPI.Validators
{
    public class CreateReservationDtoValidator : AbstractValidator<CreateReservationDto>
    {
        public CreateReservationDtoValidator()
        {
            RuleFor(r => r.Date).Must(BeValidDateAndNotInPast).WithMessage("'Date' must be a valid date and not be in the past.");
            RuleFor(r => r.Time).Must((dto, time) => BeValidTimeAndNotInPast(dto.Date, time)).WithMessage("'Time' must be a valid time and not be in the past.");
            RuleFor(r => r.PartySize).GreaterThan(0);
        }

        private bool BeValidDateAndNotInPast(DateOnly date)
        {
            return !date.Equals(default) && date >= DateOnly.FromDateTime(DateTime.Today);
        }

        private bool BeValidTimeAndNotInPast(DateOnly date, TimeOnly time)
        {
            if (date > DateOnly.FromDateTime(DateTime.Today))
            {
                return !time.Equals(default);
            }
            else if (date == DateOnly.FromDateTime(DateTime.Today))
            {
                return !time.Equals(default) && time >= TimeOnly.FromDateTime(DateTime.Now);
            }
            return false;
        }
    }
}
