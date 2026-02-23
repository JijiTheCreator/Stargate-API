using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using StargateAPI.Business.Behaviors;
using StargateAPI.Business.Commands;
using StargateAPI.Controllers;

namespace StargateAPI.Tests
{
    public class ValidationBehaviorTests
    {
        /// <summary>No validators registered — request passes through unchanged.</summary>
        [Fact]
        public async Task NoValidators_PassesThrough()
        {
            var validators = Enumerable.Empty<IValidator<CreatePerson>>();
            var behavior = new ValidationBehavior<CreatePerson, CreatePersonResult>(validators);

            var request = new CreatePerson { Name = "John Doe" };
            var expectedResult = new CreatePersonResult { Id = 1 };

            var result = await behavior.Handle(
                request,
                () => Task.FromResult(expectedResult),
                CancellationToken.None);

            Assert.Equal(1, result.Id);
        }

        /// <summary>Valid request passes through validators without throwing.</summary>
        [Fact]
        public async Task ValidRequest_PassesValidation()
        {
            var mockValidator = new Mock<IValidator<CreatePerson>>();
            mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CreatePerson>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var validators = new[] { mockValidator.Object };
            var behavior = new ValidationBehavior<CreatePerson, CreatePersonResult>(validators);

            var request = new CreatePerson { Name = "John Doe" };
            var expectedResult = new CreatePersonResult { Id = 1 };

            var result = await behavior.Handle(
                request,
                () => Task.FromResult(expectedResult),
                CancellationToken.None);

            Assert.Equal(1, result.Id);
        }

        /// <summary>Invalid request triggers ValidationException with error messages.</summary>
        [Fact]
        public async Task InvalidRequest_ThrowsValidationException()
        {
            var failure = new ValidationFailure("Name", "Person name is required.");
            var mockValidator = new Mock<IValidator<CreatePerson>>();
            mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CreatePerson>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { failure }));

            var validators = new[] { mockValidator.Object };
            var behavior = new ValidationBehavior<CreatePerson, CreatePersonResult>(validators);

            var request = new CreatePerson { Name = "" };

            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => behavior.Handle(
                    request,
                    () => Task.FromResult(new CreatePersonResult()),
                    CancellationToken.None));

            Assert.Contains(ex.Errors, e => e.ErrorMessage == "Person name is required.");
        }

        /// <summary>Multiple validators — all are executed, failures are aggregated.</summary>
        [Fact]
        public async Task MultipleValidators_AggregatesFailures()
        {
            var failure1 = new ValidationFailure("Name", "Name is required.");
            var failure2 = new ValidationFailure("Rank", "Rank is required.");

            var mockValidator1 = new Mock<IValidator<CreateAstronautDuty>>();
            mockValidator1.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CreateAstronautDuty>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { failure1 }));

            var mockValidator2 = new Mock<IValidator<CreateAstronautDuty>>();
            mockValidator2.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<CreateAstronautDuty>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { failure2 }));

            var validators = new[] { mockValidator1.Object, mockValidator2.Object };
            var behavior = new ValidationBehavior<CreateAstronautDuty, CreateAstronautDutyResult>(validators);

            var request = new CreateAstronautDuty
            {
                Name = "",
                Rank = "",
                DutyTitle = "Test",
                DutyStartDate = DateTime.Today
            };

            var ex = await Assert.ThrowsAsync<ValidationException>(
                () => behavior.Handle(
                    request,
                    () => Task.FromResult(new CreateAstronautDutyResult()),
                    CancellationToken.None));

            Assert.Equal(2, ex.Errors.Count());
        }
    }
}
