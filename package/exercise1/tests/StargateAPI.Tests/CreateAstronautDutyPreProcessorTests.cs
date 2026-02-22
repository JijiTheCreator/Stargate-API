using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Tests.Fixtures;

namespace StargateAPI.Tests
{
    public class CreateAstronautDutyPreProcessorTests
    {
        /// <summary>R2: Person must exist before receiving duty assignments.</summary>
        [Fact]
        public async Task RejectsNonExistentPerson()
        {
            using var fixture = new TestDatabaseFixture();
            var processor = new CreateAstronautDutyPreProcessor(fixture.Context);

            var request = new CreateAstronautDuty
            {
                Name = "Nobody",
                Rank = "CPT",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Today
            };

            var ex = await Assert.ThrowsAsync<BadHttpRequestException>(
                () => processor.Process(request, CancellationToken.None));

            Assert.Contains("R2", ex.Message);
        }

        /// <summary>R3: Reject duplicate duty (same title + same start date).</summary>
        [Fact]
        public async Task RejectsDuplicateDuty()
        {
            using var fixture = new TestDatabaseFixture();
            var startDate = new DateTime(2025, 6, 1);
            fixture.SeedFullAstronaut("John Doe", "1LT", "Commander", startDate);

            var processor = new CreateAstronautDutyPreProcessor(fixture.Context);

            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "1LT",
                DutyTitle = "Commander",
                DutyStartDate = startDate
            };

            var ex = await Assert.ThrowsAsync<BadHttpRequestException>(
                () => processor.Process(request, CancellationToken.None));

            Assert.Contains("R3", ex.Message);
        }

        /// <summary>R6: Retired astronaut cannot receive non-RETIRED duties.</summary>
        [Fact]
        public async Task RejectsNewDutyForRetiredPerson()
        {
            using var fixture = new TestDatabaseFixture();
            fixture.SeedPersonWithDetail("John Doe", "COL", "RETIRED");

            var processor = new CreateAstronautDutyPreProcessor(fixture.Context);

            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "COL",
                DutyTitle = "Instructor",
                DutyStartDate = DateTime.Today
            };

            var ex = await Assert.ThrowsAsync<BadHttpRequestException>(
                () => processor.Process(request, CancellationToken.None));

            Assert.Contains("R6", ex.Message);
        }

        /// <summary>R6: RETIRED duty for already-retired person is allowed (e.g. re-entry).</summary>
        [Fact]
        public async Task AllowsRetiredDutyForRetiredPerson()
        {
            using var fixture = new TestDatabaseFixture();
            fixture.SeedPersonWithDetail("John Doe", "COL", "RETIRED");

            var processor = new CreateAstronautDutyPreProcessor(fixture.Context);

            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "COL",
                DutyTitle = "RETIRED",
                DutyStartDate = DateTime.Today
            };

            await processor.Process(request, CancellationToken.None);
            // No exception means success
        }
    }
}
