using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Queries;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LogsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieve process logs from the database.
        /// </summary>
        /// <param name="count">Number of log entries to return (default 50, max 500).</param>
        /// <param name="successOnly">Optional filter: true = successes only, false = failures only.</param>
        [HttpGet("")]
        public async Task<IActionResult> GetLogs([FromQuery] int count = 50, [FromQuery] bool? successOnly = null)
        {
            try
            {
                var result = await _mediator.Send(new GetRequestLogs
                {
                    Count = count,
                    SuccessOnly = successOnly
                });

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                return this.GetResponse(new BaseResponse
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }
    }
}
