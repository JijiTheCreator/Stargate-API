using FluentValidation;
using StargateAPI.Business.Commands;

namespace StargateAPI.Business.Validators
{
    /// <summary>
    /// Validates CreatePerson requests before the handler processes them.
    /// Enforces R1 (name uniqueness) input constraints.
    /// </summary>
    public class CreatePersonValidator : AbstractValidator<CreatePerson>
    {
        public CreatePersonValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Person name is required.")
                .MaximumLength(200).WithMessage("Person name must not exceed 200 characters.")
                .Must(name => name == name?.Trim()).WithMessage("Person name must not have leading or trailing whitespace.");
        }
    }
}
