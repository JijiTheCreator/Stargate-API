using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPersonByName : IRequest<GetPersonByNameResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResult>
    {
        private readonly StargateContext _context;
        public GetPersonByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            var result = new GetPersonByNameResult();

            // BUG-2 FIX: Original query used string interpolation ($"... WHERE '{request.Name}' = a.Name"),
            // which allowed SQL injection via the name parameter. Fixed with Dapper's @Name parameterization.
            // Also switched from QueryAsync + FirstOrDefault() to QueryFirstOrDefaultAsync for efficiency —
            // the original fetched all matching rows into a list only to take the first one; this retrieves
            // at most one row directly from the database, reducing memory allocation and query overhead.
            var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronaut>(
                "SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE a.Name = @Name",
                new { request.Name });

            result.Person = person;

            return result;
        }
    }

    public class GetPersonByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
    }
}
