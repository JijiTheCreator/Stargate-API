using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using System.Net;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AstronautDutyController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AstronautDutyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetAstronautDutiesByName(string name)
        {
            try
            {
                // BUG-1 FIX: Was dispatching GetPersonByName instead of GetAstronautDutiesByName.
                // The original code returned only Person data from this endpoint, ignoring duties entirely.
                // Fixed by dispatching the correct query that joins Person + AstronautDetail + AstronautDuty,
                // matching the endpoint's stated purpose of retrieving astronaut duties by name.
                var result = await _mediator.Send(new GetAstronautDutiesByName()
                {
                    Name = name
                });

                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                return this.GetResponse(new BaseResponse()
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }            
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateAstronautDuty([FromBody] CreateAstronautDuty request)
        {
            // BUG-4 FIX: POST endpoint was missing the try-catch block that all other controller
            // actions use. Without it, any exception thrown by the handler or pre-processor
            // (e.g., BadHttpRequestException for validation failures) would result in an unhandled
            // 500 response with a stack trace instead of the structured BaseResponse error format.
            // Wrapped in the same try-catch pattern used by PersonController and the GET endpoint above.
            try
            {
                var result = await _mediator.Send(request);
                return this.GetResponse(result);
            }
            catch (Exception ex)
            {
                return this.GetResponse(new BaseResponse()
                {
                    Message = ex.Message,
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.InternalServerError
                });
            }
        }
    }
}