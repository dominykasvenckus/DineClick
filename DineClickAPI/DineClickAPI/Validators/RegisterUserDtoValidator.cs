﻿using DineClickAPI.Models;
using FluentValidation;

namespace DineClickAPI.Validators;

public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        RuleFor(u => u.Email).EmailAddress();
        RuleFor(u => u.FirstName).NotEmpty().MaximumLength(30);
        RuleFor(u => u.LastName).NotEmpty().MaximumLength(30);
        RuleFor(u => u.Role).Must(role => role != UserRole.Admin).WithMessage("'Role' has a range of values which does not include '2'.").IsInEnum();
    }
}
