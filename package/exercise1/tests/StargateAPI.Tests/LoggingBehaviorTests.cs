using Microsoft.Extensions.Logging;
using Moq;
using StargateAPI.Business.Behaviors;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using StargateAPI.Tests.Fixtures;

namespace StargateAPI.Tests
{
    public class LoggingBehaviorTests
    {
        /// <summary>Successful request logs to DB and returns response.</summary>
        [Fact]
        public async Task SuccessfulRequest_PersistsLogEntry()
        {
            using var fixture = new TestDatabaseFixture();
            var logger = new Mock<ILogger<LoggingBehavior<CreatePerson, CreatePersonResult>>>();
            var behavior = new LoggingBehavior<CreatePerson, CreatePersonResult>(logger.Object, fixture.Context);

            var request = new CreatePerson { Name = "John Doe" };
            var expectedResult = new CreatePersonResult { Id = 1 };

            var result = await behavior.Handle(
                request,
                () => Task.FromResult(expectedResult),
                CancellationToken.None);

            Assert.Equal(1, result.Id);

            // Verify a RequestLog was persisted
            var logs = fixture.Context.RequestLogs.ToList();
            Assert.Single(logs);
            Assert.Equal("CreatePerson", logs[0].RequestType);
            Assert.True(logs[0].Success);
            Assert.Equal(200, logs[0].StatusCode);
            Assert.True(logs[0].ElapsedMs >= 0);
        }

        /// <summary>Failed request logs error and re-throws exception.</summary>
        [Fact]
        public async Task FailedRequest_PersistsErrorLog_AndRethrows()
        {
            using var fixture = new TestDatabaseFixture();
            var logger = new Mock<ILogger<LoggingBehavior<CreatePerson, CreatePersonResult>>>();
            var behavior = new LoggingBehavior<CreatePerson, CreatePersonResult>(logger.Object, fixture.Context);

            var request = new CreatePerson { Name = "Test" };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => behavior.Handle(
                    request,
                    () => throw new InvalidOperationException("Test error"),
                    CancellationToken.None));

            // Verify error log was persisted
            var logs = fixture.Context.RequestLogs.ToList();
            Assert.Single(logs);
            Assert.False(logs[0].Success);
            Assert.Equal(500, logs[0].StatusCode);
            Assert.Equal("Test error", logs[0].ErrorMessage);
            Assert.NotNull(logs[0].StackTrace);
        }

        /// <summary>BaseResponse with failure status is correctly captured.</summary>
        [Fact]
        public async Task NonSuccessBaseResponse_LogsCorrectStatus()
        {
            using var fixture = new TestDatabaseFixture();
            var logger = new Mock<ILogger<LoggingBehavior<CreatePerson, CreatePersonResult>>>();
            var behavior = new LoggingBehavior<CreatePerson, CreatePersonResult>(logger.Object, fixture.Context);

            var request = new CreatePerson { Name = "Test" };
            var failedResult = new CreatePersonResult
            {
                Id = 0,
                Success = false,
                ResponseCode = 400,
                Message = "Bad request"
            };

            var result = await behavior.Handle(
                request,
                () => Task.FromResult(failedResult),
                CancellationToken.None);

            Assert.False(result.Success);

            var logs = fixture.Context.RequestLogs.ToList();
            Assert.Single(logs);
            Assert.False(logs[0].Success);
            Assert.Equal(400, logs[0].StatusCode);
        }
    }
}
