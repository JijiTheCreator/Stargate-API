using Dapper;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Tests.Fixtures;

namespace StargateAPI.Tests
{
    public class CreateAstronautDutyHandlerTests
    {
        /// <summary>R4: First duty creates AstronautDetail and new duty with null DutyEndDate.</summary>
        [Fact]
        public async Task FirstDuty_CreatesDetailAndDuty_R4()
        {
            using var fixture = new TestDatabaseFixture();
            fixture.SeedPerson("John Doe");
            var handler = new CreateAstronautDutyHandler(fixture.Context);

            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "1LT",
                DutyTitle = "Commander",
                DutyStartDate = new DateTime(2025, 6, 1)
            };

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.True(result.Success);
            Assert.NotNull(result.Id);

            // R4: New duty must have null DutyEndDate (current duty)
            var duty = fixture.Context.AstronautDuties.First(d => d.Id == result.Id);
            Assert.Null(duty.DutyEndDate);

            // Verify AstronautDetail was created
            var detail = fixture.Context.AstronautDetails.First();
            Assert.Equal("Commander", detail.CurrentDutyTitle);
            Assert.Equal("1LT", detail.CurrentRank);
            Assert.Equal(new DateTime(2025, 6, 1), detail.CareerStartDate);
        }

        /// <summary>R5: Subsequent duty closes previous duty (EndDate = NewStart - 1 day).</summary>
        [Fact]
        public async Task SubsequentDuty_ClosesPreviousDuty_R5()
        {
            using var fixture = new TestDatabaseFixture();
            fixture.SeedFullAstronaut("John Doe", "1LT", "Commander", new DateTime(2025, 1, 1));
            var handler = new CreateAstronautDutyHandler(fixture.Context);

            // Detach tracked entities so Dapper reads get fresh data
            fixture.Context.ChangeTracker.Clear();

            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "CPT",
                DutyTitle = "Pilot",
                DutyStartDate = new DateTime(2025, 7, 1)
            };

            var result = await handler.Handle(request, CancellationToken.None);
            Assert.True(result.Success);

            fixture.Context.ChangeTracker.Clear();

            // R5: Previous duty end date = new start - 1 day
            var previousDuty = fixture.Context.AstronautDuties
                .First(d => d.DutyTitle == "Commander");
            Assert.Equal(new DateTime(2025, 6, 30), previousDuty.DutyEndDate);

            // R4: New duty has null end date
            var newDuty = fixture.Context.AstronautDuties
                .First(d => d.DutyTitle == "Pilot");
            Assert.Null(newDuty.DutyEndDate);

            // Detail updated to new duty
            var detail = fixture.Context.AstronautDetails.First();
            Assert.Equal("Pilot", detail.CurrentDutyTitle);
            Assert.Equal("CPT", detail.CurrentRank);
        }

        /// <summary>R7 + BUG-3: Retired duty sets CareerEndDate = RetiredStart - 1 day (not raw date).</summary>
        [Fact]
        public async Task RetiredDuty_SetsCareerEndDate_R7_BUG3()
        {
            using var fixture = new TestDatabaseFixture();
            fixture.SeedFullAstronaut("John Doe", "COL", "Commander", new DateTime(2025, 1, 1));
            var handler = new CreateAstronautDutyHandler(fixture.Context);

            fixture.Context.ChangeTracker.Clear();

            var retiredDate = new DateTime(2025, 12, 1);
            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "COL",
                DutyTitle = "RETIRED",
                DutyStartDate = retiredDate
            };

            var result = await handler.Handle(request, CancellationToken.None);
            Assert.True(result.Success);

            fixture.Context.ChangeTracker.Clear();

            // R7 + BUG-3: CareerEndDate must be retiredDate - 1, NOT raw retiredDate
            var detail = fixture.Context.AstronautDetails.First();
            Assert.Equal(retiredDate.AddDays(-1).Date, detail.CareerEndDate);
        }

        /// <summary>R7 + BUG-3: First duty as RETIRED also sets CareerEndDate correctly.</summary>
        [Fact]
        public async Task RetiredDuty_FirstDuty_SetsCareerEndDate_R7_BUG3()
        {
            using var fixture = new TestDatabaseFixture();
            fixture.SeedPerson("John Doe");
            var handler = new CreateAstronautDutyHandler(fixture.Context);

            var retiredDate = new DateTime(2025, 3, 15);
            var request = new CreateAstronautDuty
            {
                Name = "John Doe",
                Rank = "PVT",
                DutyTitle = "RETIRED",
                DutyStartDate = retiredDate
            };

            var result = await handler.Handle(request, CancellationToken.None);
            Assert.True(result.Success);

            // R7 + BUG-3: CareerEndDate = retiredDate - 1
            var detail = fixture.Context.AstronautDetails.First();
            Assert.Equal(retiredDate.AddDays(-1).Date, detail.CareerEndDate);
        }

        /// <summary>BUG-2: Dapper queries use parameterization (no SQL injection).</summary>
        [Fact]
        public async Task DapperQueries_UseParameterizedSql_BUG2()
        {
            using var fixture = new TestDatabaseFixture();
            // Seed a person with a name that could exploit SQL injection
            fixture.SeedPerson("Robert'; DROP TABLE Person;--");
            var handler = new CreateAstronautDutyHandler(fixture.Context);

            var request = new CreateAstronautDuty
            {
                Name = "Robert'; DROP TABLE Person;--",
                Rank = "1LT",
                DutyTitle = "Test",
                DutyStartDate = DateTime.Today
            };

            // Should execute cleanly without SQL errors — name is parameterized
            var result = await handler.Handle(request, CancellationToken.None);
            Assert.True(result.Success);

            // Verify Person table still exists (not dropped)
            var people = fixture.Context.People.ToList();
            Assert.NotEmpty(people);
        }
    }
}
