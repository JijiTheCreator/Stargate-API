using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using System.Data;

namespace StargateAPI.Business.Data
{
    /// <summary>
    /// Abstraction over StargateContext for testability.
    /// Handlers and pre-processors depend on this interface, not the concrete DbContext,
    /// enabling mock/stub injection in unit tests.
    /// </summary>
    public interface IStargateContext
    {
        /// <summary>Raw ADO.NET connection for Dapper read queries.</summary>
        IDbConnection Connection { get; }

        /// <summary>EF Core DbSet for Person entities.</summary>
        DbSet<Person> People { get; }

        /// <summary>EF Core DbSet for AstronautDetail entities.</summary>
        DbSet<AstronautDetail> AstronautDetails { get; }

        /// <summary>EF Core DbSet for AstronautDuty entities.</summary>
        DbSet<AstronautDuty> AstronautDuties { get; }

        /// <summary>EF Core DbSet for process log entries.</summary>
        DbSet<RequestLog> RequestLogs { get; }

        /// <summary>Persists all tracked EF Core changes to the database.</summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
