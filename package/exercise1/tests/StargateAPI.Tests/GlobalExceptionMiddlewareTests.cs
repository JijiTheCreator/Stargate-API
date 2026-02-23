using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using StargateAPI.Middleware;
using System.Net;
using System.Text.Json;

namespace StargateAPI.Tests
{
    public class GlobalExceptionMiddlewareTests
    {
        private readonly Mock<ILogger<GlobalExceptionMiddleware>> _logger;

        public GlobalExceptionMiddlewareTests()
        {
            _logger = new Mock<ILogger<GlobalExceptionMiddleware>>();
        }

        /// <summary>Request without exception passes through normally.</summary>
        [Fact]
        public async Task NoException_PassesThrough()
        {
            var middleware = new GlobalExceptionMiddleware(
                next: (ctx) => Task.CompletedTask,
                _logger.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            Assert.Equal(200, context.Response.StatusCode);
        }

        /// <summary>BadHttpRequestException returns 400 with structured JSON.</summary>
        [Fact]
        public async Task BadHttpRequestException_Returns400()
        {
            var middleware = new GlobalExceptionMiddleware(
                next: (ctx) => throw new BadHttpRequestException("Person not found"),
                _logger.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            // Read response body
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var json = JsonDocument.Parse(body);
            Assert.False(json.RootElement.GetProperty("success").GetBoolean());
            Assert.Contains("Person not found", json.RootElement.GetProperty("message").GetString());
        }

        /// <summary>FluentValidation.ValidationException returns 400 with aggregated errors.</summary>
        [Fact]
        public async Task ValidationException_Returns400_WithErrors()
        {
            var failures = new[]
            {
                new FluentValidation.Results.ValidationFailure("Name", "Name is required."),
                new FluentValidation.Results.ValidationFailure("Rank", "Rank is required.")
            };
            var validationEx = new FluentValidation.ValidationException(failures);

            var middleware = new GlobalExceptionMiddleware(
                next: (ctx) => throw validationEx,
                _logger.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            Assert.Equal((int)HttpStatusCode.BadRequest, context.Response.StatusCode);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
            Assert.Contains("Name is required", body);
            Assert.Contains("Rank is required", body);
        }

        /// <summary>Unhandled exception returns 500 with generic error message.</summary>
        [Fact]
        public async Task UnhandledException_Returns500_NoStackTrace()
        {
            var middleware = new GlobalExceptionMiddleware(
                next: (ctx) => throw new InvalidOperationException("Database connection failed"),
                _logger.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
            // Should NOT leak the internal error message
            Assert.DoesNotContain("Database connection failed", body);
            Assert.Contains("unexpected error", body);
        }
    }
}
