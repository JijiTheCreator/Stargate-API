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

            if (person is null) throw new BadHttpRequestException("Bad Request");

            var verifyNoPreviousDuty = _context.AstronautDuties.FirstOrDefault(z => z.DutyTitle == request.DutyTitle && z.DutyStartDate == request.DutyStartDate);

            if (verifyNoPreviousDuty is not null) throw new BadHttpRequestException("Bad Request");

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
                    // BUG-3 FIX: Original code set CareerEndDate = DutyStartDate.Date (the raw start date).
                    // Per Rule R7, CareerEndDate should be one day before the RETIRED duty begins, because
                    // the career ends the day before retirement takes effect — consistent with how existing
                    // duty DutyEndDate is calculated (new start - 1 day). The "else" branch already had
                    // AddDays(-1) but this "new astronaut" branch did not, creating an inconsistency.
                    astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
                }

                await _context.AstronautDetails.AddAsync(astronautDetail);
            }
            else
            {
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                if (request.DutyTitle == "RETIRED")
                {
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
                astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
                _context.AstronautDuties.Update(astronautDuty);
            }

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
