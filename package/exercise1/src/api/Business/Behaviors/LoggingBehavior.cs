using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Diagnostics;
using System.Text.Json;

namespace StargateAPI.Business.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior that logs every request and response to both
    /// ILogger (console/Serilog) and the RequestLog database table.
    /// Captures timing, success/failure, and exception details.
    /// </summary>
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        private readonly IStargateContext _context;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IStargateContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestType = typeof(TRequest).Name;
            var requestBody = SerializeSafe(request);
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation("Processing {RequestType}: {RequestBody}", requestType, requestBody);

            try
            {
                var response = await next();
                stopwatch.Stop();

                // Extract status info from BaseResponse if available
                var success = true;
                var statusCode = 200;
                string? responseBody = null;

                if (response is BaseResponse baseResponse)
                {
                    success = baseResponse.Success;
                    statusCode = baseResponse.ResponseCode;
                    responseBody = SerializeSafe(response, maxLength: 500);
                }

                _logger.LogInformation(
                    "Completed {RequestType} in {ElapsedMs}ms — Success: {Success}, Status: {StatusCode}",
                    requestType, stopwatch.ElapsedMilliseconds, success, statusCode);

                // Persist to database
                await PersistLog(new RequestLog
                {
                    RequestType = requestType,
                    RequestBody = requestBody,
                    ResponseBody = responseBody,
                    StatusCode = statusCode,
                    Success = success,
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Timestamp = DateTime.UtcNow
                });

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex,
                    "Failed {RequestType} in {ElapsedMs}ms — {ErrorMessage}",
                    requestType, stopwatch.ElapsedMilliseconds, ex.Message);

                // Persist failure to database
                await PersistLog(new RequestLog
                {
                    RequestType = requestType,
                    RequestBody = requestBody,
                    StatusCode = 500,
                    Success = false,
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    ElapsedMs = stopwatch.ElapsedMilliseconds,
                    Timestamp = DateTime.UtcNow
                });

                throw; // Re-throw for GlobalExceptionMiddleware to handle
            }
        }

        private async Task PersistLog(RequestLog log)
        {
            try
            {
                _context.RequestLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log persistence failure should never crash the request pipeline
                _logger.LogWarning(ex, "Failed to persist RequestLog for {RequestType}", log.RequestType);
            }
        }

        private static string? SerializeSafe(object? obj, int maxLength = 1000)
        {
            if (obj == null) return null;
            try
            {
                var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
                return json.Length > maxLength ? json[..maxLength] + "..." : json;
            }
            catch
            {
                return obj.GetType().Name;
            }
        }
    }
}
