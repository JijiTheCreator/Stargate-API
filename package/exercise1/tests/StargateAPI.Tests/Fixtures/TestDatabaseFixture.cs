using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;

namespace StargateAPI.Tests.Fixtures
{
    /// <summary>
    /// Creates a fresh in-memory SQLite database for each test.
    /// Supports both EF Core (DbSet) and Dapper (Connection) code paths.
    /// </summary>
    public class TestDatabaseFixture : IDisposable
    {
        private readonly SqliteConnection _connection;
        public StargateContext Context { get; }

        public TestDatabaseFixture()
        {
            // In-memory SQLite requires an open connection to persist across queries
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new StargateContext(options);
            Context.Database.EnsureCreated();
        }

        /// <summary>Seeds a Person and returns its Id.</summary>
        public int SeedPerson(string name = "John Doe")
        {
            var person = new Person { Name = name };
            Context.People.Add(person);
            Context.SaveChanges();
            return person.Id;
        }

        /// <summary>Seeds a Person + AstronautDetail and returns (PersonId, DetailId).</summary>
        public (int PersonId, int DetailId) SeedPersonWithDetail(
            string name = "John Doe",
            string rank = "1LT",
            string dutyTitle = "Commander",
            DateTime? careerStart = null)
        {
            var personId = SeedPerson(name);

            var detail = new AstronautDetail
            {
                PersonId = personId,
                CurrentRank = rank,
                CurrentDutyTitle = dutyTitle,
                CareerStartDate = careerStart ?? new DateTime(2025, 1, 1)
            };

            Context.AstronautDetails.Add(detail);
            Context.SaveChanges();

            return (personId, detail.Id);
        }

        /// <summary>Seeds a Person + AstronautDetail + AstronautDuty and returns all Ids.</summary>
        public (int PersonId, int DetailId, int DutyId) SeedFullAstronaut(
            string name = "John Doe",
            string rank = "1LT",
            string dutyTitle = "Commander",
            DateTime? dutyStart = null)
        {
            var (personId, detailId) = SeedPersonWithDetail(name, rank, dutyTitle, dutyStart);

            var duty = new AstronautDuty
            {
                PersonId = personId,
                Rank = rank,
                DutyTitle = dutyTitle,
                DutyStartDate = dutyStart ?? new DateTime(2025, 1, 1),
                DutyEndDate = null
            };

            Context.AstronautDuties.Add(duty);
            Context.SaveChanges();

            return (personId, detailId, duty.Id);
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}
