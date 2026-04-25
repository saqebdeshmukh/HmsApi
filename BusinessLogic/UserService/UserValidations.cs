using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.UserService
{
    public class UserValidations: AbstractValidator<UserRequestModel>
    {
        public UserValidations()
        {
            RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches("[A-Z]").WithMessage("Password must contain uppercase letter")
                .Matches("[0-9]").WithMessage("Password must contain a number");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required");
        }
    }
}
