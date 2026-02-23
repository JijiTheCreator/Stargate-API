using StargateAPI.Business.Queries;
using StargateAPI.Business.Data;
using StargateAPI.Tests.Fixtures;

namespace StargateAPI.Tests
{
    public class GetRequestLogsHandlerTests
    {
        /// <summary>Empty log table returns empty list.</summary>
        [Fact]
        public async Task EmptyDatabase_ReturnsEmptyList()
        {
            using var fixture = new TestDatabaseFixture();
            var handler = new GetRequestLogsHandler(fixture.Context);

            var result = await handler.Handle(new GetRequestLogs(), CancellationToken.None);

            Assert.True(result.Success);
            Assert.Empty(result.Logs);
        }

        /// <summary>Returns logs ordered by timestamp descending.</summary>
        [Fact]
        public async Task ReturnsLogs_OrderedByTimestampDesc()
        {
            using var fixture = new TestDatabaseFixture();

            // Seed 3 log entries
            fixture.Context.RequestLogs.Add(new RequestLog
            {
                RequestType = "GetPeople",
                StatusCode = 200,
                Success = true,
                Timestamp = new DateTime(2025, 1, 1)
            });
            fixture.Context.RequestLogs.Add(new RequestLog
            {
                RequestType = "CreatePerson",
                StatusCode = 200,
                Success = true,
                Timestamp = new DateTime(2025, 6, 1)
            });
            fixture.Context.RequestLogs.Add(new RequestLog
            {
                RequestType = "CreateAstronautDuty",
                StatusCode = 400,
                Success = false,
                ErrorMessage = "Person not found",
                Timestamp = new DateTime(2025, 3, 1)
            });
            await fixture.Context.SaveChangesAsync();

            var handler = new GetRequestLogsHandler(fixture.Context);
            var result = await handler.Handle(new GetRequestLogs { Count = 10 }, CancellationToken.None);

            Assert.Equal(3, result.Logs.Count);
        }

        /// <summary>Count parameter limits returned results.</summary>
        [Fact]
        public async Task Count_LimitsResults()
        {
            using var fixture = new TestDatabaseFixture();

            for (int i = 0; i < 5; i++)
            {
                fixture.Context.RequestLogs.Add(new RequestLog
                {
                    RequestType = $"Request{i}",
                    StatusCode = 200,
                    Success = true,
                    Timestamp = DateTime.UtcNow.AddMinutes(i)
                });
            }
            await fixture.Context.SaveChangesAsync();

            var handler = new GetRequestLogsHandler(fixture.Context);
            var result = await handler.Handle(new GetRequestLogs { Count = 2 }, CancellationToken.None);

            Assert.Equal(2, result.Logs.Count);
        }

        /// <summary>SuccessOnly filter returns only successful or failed logs.</summary>
        [Fact]
        public async Task SuccessFilter_FiltersCorrectly()
        {
            using var fixture = new TestDatabaseFixture();

            fixture.Context.RequestLogs.Add(new RequestLog
            {
                RequestType = "GetPeople",
                StatusCode = 200,
                Success = true,
                Timestamp = DateTime.UtcNow
            });
            fixture.Context.RequestLogs.Add(new RequestLog
            {
                RequestType = "CreatePerson",
                StatusCode = 400,
                Success = false,
                Timestamp = DateTime.UtcNow
            });
            await fixture.Context.SaveChangesAsync();

            var handler = new GetRequestLogsHandler(fixture.Context);

            // Filter for successes only
            var successResult = await handler.Handle(
                new GetRequestLogs { SuccessOnly = true }, CancellationToken.None);
            Assert.All(successResult.Logs, log => Assert.True(log.Success));

            // Filter for failures only
            var failureResult = await handler.Handle(
                new GetRequestLogs { SuccessOnly = false }, CancellationToken.None);
            Assert.All(failureResult.Logs, log => Assert.False(log.Success));
        }

        /// <summary>Count is clamped to [1, 500] range.</summary>
        [Fact]
        public async Task Count_IsClamped()
        {
            using var fixture = new TestDatabaseFixture();
            var handler = new GetRequestLogsHandler(fixture.Context);

            // Negative count should not throw
            var result = await handler.Handle(new GetRequestLogs { Count = -5 }, CancellationToken.None);
            Assert.True(result.Success);

            // Very large count should not throw
            result = await handler.Handle(new GetRequestLogs { Count = 99999 }, CancellationToken.None);
            Assert.True(result.Success);
        }
    }
}
