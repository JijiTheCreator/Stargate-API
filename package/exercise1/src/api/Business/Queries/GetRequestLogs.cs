using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetRequestLogs : IRequest<GetRequestLogsResult>
    {
        /// <summary>Maximum number of log entries to return (default 50, max 500).</summary>
        public int Count { get; set; } = 50;

        /// <summary>Optional filter: true = only successes, false = only failures, null = all.</summary>
        public bool? SuccessOnly { get; set; }
    }

    public class GetRequestLogsHandler : IRequestHandler<GetRequestLogs, GetRequestLogsResult>
    {
        // CONVENTION: Dapper for read operations (query performance)
        private readonly IStargateContext _context;

        public GetRequestLogsHandler(IStargateContext context)
        {
            _context = context;
        }

        public async Task<GetRequestLogsResult> Handle(GetRequestLogs request, CancellationToken cancellationToken)
        {
            var result = new GetRequestLogsResult();

            // Clamp count to [1, 500]
            var count = Math.Clamp(request.Count, 1, 500);

            string query;
            object parameters;

            if (request.SuccessOnly.HasValue)
            {
                query = "SELECT * FROM [RequestLog] WHERE Success = @Success ORDER BY Timestamp DESC LIMIT @Count";
                parameters = new { Success = request.SuccessOnly.Value, Count = count };
            }
            else
            {
                query = "SELECT * FROM [RequestLog] ORDER BY Timestamp DESC LIMIT @Count";
                parameters = new { Count = count };
            }

            var logs = await _context.Connection.QueryAsync<RequestLog>(query, parameters);
            result.Logs = logs.ToList();

            return result;
        }
    }

    public class GetRequestLogsResult : BaseResponse
    {
        public List<RequestLog> Logs { get; set; } = new List<RequestLog>();
    }
}
