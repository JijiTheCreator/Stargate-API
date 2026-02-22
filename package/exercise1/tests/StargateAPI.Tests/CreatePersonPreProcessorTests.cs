using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Commands;
using StargateAPI.Tests.Fixtures;

namespace StargateAPI.Tests
{
    public class CreatePersonPreProcessorTests
    {
        /// <summary>R1: Duplicate name must be rejected.</summary>
        [Fact]
        public async Task RejectsDuplicateName()
        {
            using var fixture = new TestDatabaseFixture();
            fixture.SeedPerson("John Doe");
            var processor = new CreatePersonPreProcessor(fixture.Context);

            var request = new CreatePerson { Name = "John Doe" };

            var ex = await Assert.ThrowsAsync<BadHttpRequestException>(
                () => processor.Process(request, CancellationToken.None));

            Assert.Contains("R1", ex.Message);
        }

        /// <summary>R1: New unique name must succeed.</summary>
        [Fact]
        public async Task AllowsNewUniqueName()
        {
            using var fixture = new TestDatabaseFixture();
            fixture.SeedPerson("John Doe");
            var processor = new CreatePersonPreProcessor(fixture.Context);

            var request = new CreatePerson { Name = "Jane Doe" };

            await processor.Process(request, CancellationToken.None);
            // No exception means success
        }
    }
}
