using Dapper;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using StargateAPI.Tests.Fixtures;

namespace StargateAPI.Tests
{
    public class GetAstronautDutiesByNameHandlerTests
    {
        /// <summary>BUG-5: Null person returns empty result without NullReferenceException.</summary>
        [Fact]
        public async Task NullPerson_ReturnsEmptyResult_BUG5()
        {
            using var fixture = new TestDatabaseFixture();
            var handler = new GetAstronautDutiesByNameHandler(fixture.Context);

            var result = await handler.Handle(
                new GetAstronautDutiesByName { Name = "Nobody" },
                CancellationToken.None);

            // BUG-5: Must not throw, must return empty
            Assert.Null(result.Person);
            Assert.Empty(result.AstronautDuties);
        }

        /// <summary>Valid person returns person data and duties.</summary>
        [Fact]
        public async Task ValidPerson_ReturnsDuties()
        {
            using var fixture = new TestDatabaseFixture();
            fixture.SeedFullAstronaut("John Doe", "1LT", "Commander", new DateTime(2025, 1, 1));

            // Clear tracker so Dapper reads fresh data
            fixture.Context.ChangeTracker.Clear();

            var handler = new GetAstronautDutiesByNameHandler(fixture.Context);

            var result = await handler.Handle(
                new GetAstronautDutiesByName { Name = "John Doe" },
                CancellationToken.None);

            Assert.NotNull(result.Person);
            Assert.Equal("John Doe", result.Person.Name);
            Assert.Single(result.AstronautDuties);
        }

        /// <summary>BUG-2: Name with special characters doesn't cause SQL error.</summary>
        [Fact]
        public async Task ParameterizedQuery_SpecialChars_BUG2()
        {
            using var fixture = new TestDatabaseFixture();
            fixture.SeedFullAstronaut("O'Brien", "MAJ", "Engineer", new DateTime(2025, 1, 1));

            fixture.Context.ChangeTracker.Clear();

            var handler = new GetAstronautDutiesByNameHandler(fixture.Context);

            var result = await handler.Handle(
                new GetAstronautDutiesByName { Name = "O'Brien" },
                CancellationToken.None);

            // Parameterized query handles apostrophe safely
            Assert.NotNull(result.Person);
            Assert.Equal("O'Brien", result.Person.Name);
        }
    }
}
