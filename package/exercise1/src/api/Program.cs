using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StargateAPI.Business.Behaviors;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 4.3: Serilog for structured console logging
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .WriteTo.Console());

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3.4: CORS configuration for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
        policy.WithOrigins(
                  "http://localhost:4200",  // Angular dev server
                  "http://localhost"        // Docker nginx UI
              )
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Database context (EF Core + SQLite)
builder.Services.AddDbContext<StargateContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("StarbaseApiDatabase")));

// 3.5: Register IStargateContext for testability — handlers depend on the interface
builder.Services.AddScoped<IStargateContext>(sp => sp.GetRequiredService<StargateContext>());

// 3.2: Register FluentValidation validators from this assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// MediatR with pre-processors, validation, and logging behaviors
builder.Services.AddMediatR(cfg =>
{
    cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
    // Pipeline order: Logging → Validation → Handler
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

var app = builder.Build();

// 7.4: Auto-apply EF Core migrations on startup (creates DB if missing)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StargateContext>();
    db.Database.Migrate();
}

// Swagger enabled in all environments (accessible in Docker)
app.UseSwagger();
app.UseSwaggerUI();

// 3.1: Global exception handling — catches unhandled exceptions and returns structured JSON
app.UseMiddleware<GlobalExceptionMiddleware>();

// 4.3: Serilog request logging
app.UseSerilogRequestLogging();

// HTTPS redirection disabled — Docker runs HTTP internally; TLS handled at reverse proxy
// app.UseHttpsRedirection();

// 3.4: Enable CORS before authorization
app.UseCors("AllowAngularDev");

app.UseAuthorization();

app.MapControllers();

// 7.5: Health check endpoint for Docker healthcheck
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
