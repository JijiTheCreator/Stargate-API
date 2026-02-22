using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;

        public GetAstronautDutiesByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
            var result = new GetAstronautDutiesByNameResult();

            // BUG-2 FIX: Original query used string interpolation ($"... WHERE '{request.Name}' = a.Name"),
            // embedding user input directly into SQL — a textbook SQL injection vulnerability.
            // Fixed with Dapper's @Name parameterization to bind the value safely.
            var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronaut>(
                "SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate FROM [Person] a LEFT JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE a.Name = @Name",
                new { request.Name });

            // BUG-5 FIX: Original code accessed person.PersonId unconditionally after the query.
            // If the person doesn't exist, QueryFirstOrDefaultAsync returns null, and accessing
            // person.PersonId throws a NullReferenceException (unhandled 500 error).
            // Fixed with an early return of an empty result — the caller receives a valid response
            // with null Person and an empty duties list, which is the correct semantic for "not found".
            if (person == null)
            {
                return result;
            }

            result.Person = person;

            // BUG-2 FIX: Same SQL injection pattern — person.PersonId was interpolated into the query.
            // Replaced with parameterized @PersonId.
            var duties = await _context.Connection.QueryAsync<AstronautDuty>(
                "SELECT * FROM [AstronautDuty] WHERE PersonId = @PersonId ORDER BY DutyStartDate DESC",
                new { person.PersonId });

            result.AstronautDuties = duties.ToList();

            return result;
        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut? Person { get; set; }
        public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
    }
}
