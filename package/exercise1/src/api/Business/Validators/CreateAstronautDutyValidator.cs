using FluentValidation;
using StargateAPI.Business.Commands;

namespace StargateAPI.Business.Validators
{
    /// <summary>
    /// Validates CreateAstronautDuty requests before the handler processes them.
    /// Enforces input constraints for R2/R3/R6 rule validations.
    /// </summary>
    public class CreateAstronautDutyValidator : AbstractValidator<CreateAstronautDuty>
    {
        public CreateAstronautDutyValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Astronaut name is required.")
                .MaximumLength(200).WithMessage("Astronaut name must not exceed 200 characters.");

            RuleFor(x => x.Rank)
                .NotEmpty().WithMessage("Rank is required.")
                .MaximumLength(50).WithMessage("Rank must not exceed 50 characters.");

            RuleFor(x => x.DutyTitle)
                .NotEmpty().WithMessage("Duty title is required.")
                .MaximumLength(100).WithMessage("Duty title must not exceed 100 characters.");

            RuleFor(x => x.DutyStartDate)
                .NotEqual(default(DateTime)).WithMessage("Duty start date is required and must be a valid date.");
        }
    }
}
