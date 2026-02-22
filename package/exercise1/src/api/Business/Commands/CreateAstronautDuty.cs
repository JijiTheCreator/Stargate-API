using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            // R2: A Person must exist before they can receive astronaut duty assignments.
            // Without a valid Person record, there is no entity to attach duties to.
            if (person is null)
                throw new BadHttpRequestException($"Person '{request.Name}' not found. (Rule R2: Person must exist)");

            // R3: A Person will only ever hold one current duty at a time.
            // Reject exact duplicate duty assignments (same title + same start date).
            var verifyNoPreviousDuty = _context.AstronautDuties.FirstOrDefault(z => z.DutyTitle == request.DutyTitle && z.DutyStartDate == request.DutyStartDate);

            if (verifyNoPreviousDuty is not null)
                throw new BadHttpRequestException($"Duty '{request.DutyTitle}' starting {request.DutyStartDate:yyyy-MM-dd} already exists for '{request.Name}'. (Rule R3: One duty at a time)");

            // R6: A retired astronaut cannot receive new non-RETIRED duty assignments.
            // Once a Person's current duty is RETIRED, the only valid assignment is another RETIRED entry.
            var astronautDetail = _context.AstronautDetails.AsNoTracking().FirstOrDefault(z => z.PersonId == person.Id);
            if (astronautDetail != null && astronautDetail.CurrentDutyTitle == "RETIRED" && request.DutyTitle != "RETIRED")
                throw new BadHttpRequestException($"Person '{request.Name}' is retired and cannot receive new duties. (Rule R6: Retirement is permanent)");

            return Task.CompletedTask;
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            // BUG-2 FIX: Original query used string interpolation ($"... WHERE '{request.Name}' = Name"),
            // which is a critical SQL injection vulnerability — user-supplied input was embedded directly
            // into the SQL string. Fixed by using Dapper's parameterized query syntax (@Name) with an
            // anonymous object, which ensures values are passed as bound parameters, never interpolated.
            var person = await _context.Connection.QueryFirstOrDefaultAsync<Person>(
                "SELECT * FROM [Person] WHERE Name = @Name", new { request.Name });

            // BUG-2 FIX: Same SQL injection pattern — person.Id was interpolated directly into the query.
            // While person.Id is an int (lower injection risk), parameterization is enforced universally
            // as a defense-in-depth measure to prevent any future refactoring from introducing risk.
            var astronautDetail = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDetail>(
                "SELECT * FROM [AstronautDetail] WHERE PersonId = @PersonId", new { PersonId = person.Id });

            if (astronautDetail == null)
            {
                astronautDetail = new AstronautDetail();
                astronautDetail.PersonId = person.Id;
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                astronautDetail.CareerStartDate = request.DutyStartDate.Date;
                if (request.DutyTitle == "RETIRED")
                {
                    // BUG-3 FIX / R7: Career end date is one day before the RETIRED duty start date.
                    // Original code set CareerEndDate = DutyStartDate.Date (the raw start date).
                    // Per Rule R7, CareerEndDate should be one day before retirement takes effect,
                    // consistent with the "else" branch and with how DutyEndDate is calculated (R5).
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                }

                await _context.AstronautDetails.AddAsync(astronautDetail);
            }
            else
            {
                // R3: Overwrite current duty snapshot — a Person holds only one current duty at a time.
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                if (request.DutyTitle == "RETIRED")
                {
                    // R6 + R7: Mark career as ended one day before the RETIRED duty start date.
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                }
                _context.AstronautDetails.Update(astronautDetail);
            }

            // BUG-2 FIX: Same SQL injection pattern as above — person.Id interpolated into query string.
            // Replaced with parameterized @PersonId for consistency and security.
            var astronautDuty = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDuty>(
                "SELECT * FROM [AstronautDuty] WHERE PersonId = @PersonId ORDER BY DutyStartDate DESC",
                new { PersonId = person.Id });

            if (astronautDuty != null)
            {
                // R5: Previous duty end date is set to the day before the new duty start date.
                // This closes the previous duty assignment when a new one is received.
                astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                _context.AstronautDuties.Update(astronautDuty);
            }

            // R4: A Person's current duty will not have a Duty End Date (null).
            // New duty records are always created with DutyEndDate = null, establishing them
            // as the current active duty. The previous duty is closed above (R5).
            var newAstronautDuty = new AstronautDuty()
            {
                PersonId = person.Id,
                Rank = request.Rank,
                DutyTitle = request.DutyTitle,
                DutyStartDate = request.DutyStartDate.Date,
                DutyEndDate = null
            };

            await _context.AstronautDuties.AddAsync(newAstronautDuty);

            await _context.SaveChangesAsync();

            return new CreateAstronautDutyResult()
            {
                Id = newAstronautDuty.Id
            };
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
