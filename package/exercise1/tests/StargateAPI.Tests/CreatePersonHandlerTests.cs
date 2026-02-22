using StargateAPI.Business.Commands;
using StargateAPI.Tests.Fixtures;

namespace StargateAPI.Tests
{
    public class CreatePersonHandlerTests
    {
        [Fact]
        public async Task CreatesPersonAndReturnsId()
        {
            using var fixture = new TestDatabaseFixture();
            var handler = new CreatePersonHandler(fixture.Context);

            var result = await handler.Handle(
                new CreatePerson { Name = "Jane Doe" },
                CancellationToken.None);

            Assert.True(result.Id > 0);
            Assert.True(result.Success);

            // Verify person exists in DB
            var person = fixture.Context.People.FirstOrDefault(p => p.Name == "Jane Doe");
            Assert.NotNull(person);
            Assert.Equal(result.Id, person.Id);
        }
    }
}
