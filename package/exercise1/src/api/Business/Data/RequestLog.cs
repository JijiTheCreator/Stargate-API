using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    /// <summary>
    /// Represents a log entry for an API request processed through the MediatR pipeline.
    /// Persisted to the SQLite database for queryable process logging (Task T5).
    /// </summary>
    [Table("RequestLog")]
    public class RequestLog
    {
        public int Id { get; set; }

        /// <summary>The MediatR request type name, e.g. "CreatePerson", "GetAstronautDutiesByName".</summary>
        public string RequestType { get; set; } = string.Empty;

        /// <summary>JSON-serialized request body (may be null for simple queries).</summary>
        public string? RequestBody { get; set; }

        /// <summary>JSON-serialized response body (truncated to prevent storage bloat).</summary>
        public string? ResponseBody { get; set; }

        /// <summary>HTTP-equivalent status code for the operation result.</summary>
        public int StatusCode { get; set; }

        /// <summary>Whether the operation completed successfully.</summary>
        public bool Success { get; set; }

        /// <summary>Exception message if the operation failed.</summary>
        public string? ErrorMessage { get; set; }

        /// <summary>Stack trace if an exception was thrown.</summary>
        public string? StackTrace { get; set; }

        /// <summary>Elapsed time in milliseconds for the handler execution.</summary>
        public long ElapsedMs { get; set; }

        /// <summary>UTC timestamp when the request was received.</summary>
        public DateTime Timestamp { get; set; }
    }

    public class RequestLogConfiguration : IEntityTypeConfiguration<RequestLog>
    {
        public void Configure(EntityTypeBuilder<RequestLog> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.HasIndex(x => x.Timestamp);
            builder.HasIndex(x => x.RequestType);
        }
    }
}
