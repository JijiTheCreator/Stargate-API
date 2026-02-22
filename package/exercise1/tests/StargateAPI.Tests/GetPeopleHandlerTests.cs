using StargateAPI.Business.Queries;
using StargateAPI.Tests.Fixtures;

namespace StargateAPI.Tests
{
    public class GetPeopleHandlerTests
    {
        [Fact]
        public async Task EmptyDb_ReturnsEmptyList()
        {
            using var fixture = new TestDatabaseFixture();
            var handler = new GetPeopleHandler(fixture.Context);

            var result = await handler.Handle(
                new GetPeople(),
                CancellationToken.None);

            Assert.Empty(result.People);
        }

        [Fact]
        public async Task PopulatedDb_ReturnsAllPeople()
        {
            using var fixture = new TestDatabaseFixture();
            fixture.SeedPerson("John Doe");
            fixture.SeedPerson("Jane Doe");

            fixture.Context.ChangeTracker.Clear();

            var handler = new GetPeopleHandler(fixture.Context);

            var result = await handler.Handle(
                new GetPeople(),
                CancellationToken.None);

            Assert.Equal(2, result.People.Count);
        }
    }
}
