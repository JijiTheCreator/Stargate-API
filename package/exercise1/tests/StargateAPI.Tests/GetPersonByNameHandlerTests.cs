using StargateAPI.Business.Queries;
using StargateAPI.Tests.Fixtures;

namespace StargateAPI.Tests
{
    public class GetPersonByNameHandlerTests
    {
        /// <summary>BUG-2: Parameterized query returns correct person.</summary>
        [Fact]
        public async Task ExistingPerson_ReturnsPersonData_BUG2()
        {
            using var fixture = new TestDatabaseFixture();
            fixture.SeedPersonWithDetail("John Doe", "1LT", "Commander");

            fixture.Context.ChangeTracker.Clear();

            var handler = new GetPersonByNameHandler(fixture.Context);

            var result = await handler.Handle(
                new GetPersonByName { Name = "John Doe" },
                CancellationToken.None);

            Assert.NotNull(result.Person);
            Assert.Equal("John Doe", result.Person.Name);
            Assert.Equal("Commander", result.Person.CurrentDutyTitle);
        }

        [Fact]
        public async Task NonExistentPerson_ReturnsNull()
        {
            using var fixture = new TestDatabaseFixture();
            var handler = new GetPersonByNameHandler(fixture.Context);

            var result = await handler.Handle(
                new GetPersonByName { Name = "Nobody" },
                CancellationToken.None);

            Assert.Null(result.Person);
        }
    }
}
