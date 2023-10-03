﻿using FluentValidation;

namespace DineClickAPI.Validators
{
    public class UpdateReservationDtoValidator : AbstractValidator<UpdateReservationDto>
    {
        public UpdateReservationDtoValidator()
        {
            RuleFor(r => r.Date).Must(BeValidDateAndNotInPast).WithMessage("'Date' must be a valid date and not be in the past.");
            RuleFor(r => r.Time).Must(BeValidTimeAndNotInPast).WithMessage("'Time' must be a valid time and not be in the past.");
            RuleFor(r => r.PartySize).GreaterThan(0);
            RuleFor(r => r.ReservationStatus).IsInEnum();
        }

        private bool BeValidDateAndNotInPast(DateOnly date)
        {
            return !date.Equals(default) && date >= DateOnly.FromDateTime(DateTime.Today);
        }

        private bool BeValidTimeAndNotInPast(TimeOnly time)
        {
            return !time.Equals(default) && time >= TimeOnly.FromDateTime(DateTime.Now);
        }
    }
}
