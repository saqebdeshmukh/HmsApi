using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModels;

namespace BusinessLogic.PatientService
{
    public class PatientValidation : AbstractValidator<PatientRequestModel>
    {
        public PatientValidation() 
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.");
            RuleFor(x => x.DateOfBirth).NotEmpty().WithMessage("Date of birth is required.");
            RuleFor(x => x.Gender).NotEmpty().WithMessage("Gender is required.");
            RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone number is required.");
        }
    }
}
