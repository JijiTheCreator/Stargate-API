using StargateAPI.Business.Validators;
using StargateAPI.Business.Commands;

namespace StargateAPI.Tests
{
    public class ValidatorTests
    {
        // --- CreatePersonValidator ---

        /// <summary>Empty name fails validation.</summary>
        [Fact]
        public async Task CreatePerson_EmptyName_Fails()
        {
            var validator = new CreatePersonValidator();
            var result = await validator.ValidateAsync(new CreatePerson { Name = "" });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("required"));
        }

        /// <summary>Name exceeding 200 chars fails validation.</summary>
        [Fact]
        public async Task CreatePerson_LongName_Fails()
        {
            var validator = new CreatePersonValidator();
            var result = await validator.ValidateAsync(new CreatePerson { Name = new string('A', 201) });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("200"));
        }

        /// <summary>Name with whitespace fails validation.</summary>
        [Fact]
        public async Task CreatePerson_WhitespaceName_Fails()
        {
            var validator = new CreatePersonValidator();
            var result = await validator.ValidateAsync(new CreatePerson { Name = " John " });
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("whitespace"));
        }

        /// <summary>Valid name passes validation.</summary>
        [Fact]
        public async Task CreatePerson_ValidName_Passes()
        {
            var validator = new CreatePersonValidator();
            var result = await validator.ValidateAsync(new CreatePerson { Name = "John Doe" });
            Assert.True(result.IsValid);
        }

        // --- CreateAstronautDutyValidator ---

        /// <summary>Empty fields fail validation.</summary>
        [Fact]
        public async Task CreateDuty_EmptyFields_Fails()
        {
            var validator = new CreateAstronautDutyValidator();
            var result = await validator.ValidateAsync(new CreateAstronautDuty
            {
                Name = "",
                Rank = "",
                DutyTitle = "",
                DutyStartDate = default
            });

            Assert.False(result.IsValid);
            Assert.True(result.Errors.Count >= 4); // Name, Rank, DutyTitle, DutyStartDate
        }

        /// <summary>Valid duty request passes validation.</summary>
        [Fact]
        public async Task CreateDuty_ValidRequest_Passes()
        {
            var validator = new CreateAstronautDutyValidator();
            var result = await validator.ValidateAsync(new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "CPT",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 6, 1)
            });

            Assert.True(result.IsValid);
        }

        /// <summary>Name exceeding 200 chars fails.</summary>
        [Fact]
        public async Task CreateDuty_LongName_Fails()
        {
            var validator = new CreateAstronautDutyValidator();
            var result = await validator.ValidateAsync(new CreateAstronautDuty
            {
                Name = new string('A', 201),
                Rank = "CPT",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Today
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        }

        /// <summary>Rank exceeding 50 chars fails.</summary>
        [Fact]
        public async Task CreateDuty_LongRank_Fails()
        {
            var validator = new CreateAstronautDutyValidator();
            var result = await validator.ValidateAsync(new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = new string('R', 51),
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Today
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Rank");
        }

        /// <summary>DutyTitle exceeding 100 chars fails.</summary>
        [Fact]
        public async Task CreateDuty_LongDutyTitle_Fails()
        {
            var validator = new CreateAstronautDutyValidator();
            var result = await validator.ValidateAsync(new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "CPT",
                DutyTitle = new string('T', 101),
                DutyStartDate = DateTime.Today
            });

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "DutyTitle");
        }
    }
}
