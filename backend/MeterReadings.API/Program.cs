using FluentValidation;
using FluentValidation.AspNetCore;
using MeterReadings.Core.DTOs;
using MeterReadings.Core.Interfaces.Repositories;
using MeterReadings.Core.Interfaces.Services;
using MeterReadings.Core.Validators;
using MeterReadings.Data.Context;
using MeterReadings.Data.Repositories;
using MeterReadings.Data.Extensions;
using MeterReadings.Data.Seed;
using MeterReadings.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

// Configure Npgsql to handle DateTime conversions globally before any DbContext is initialized
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<MeterReadingDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure());
});

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();
builder.Services.AddScoped<Seeder>();

builder.Services.AddScoped<ICsvParserService, CsvParserService>();
builder.Services.AddScoped<IMeterReadingService, MeterReadingService>();
builder.Services.AddScoped<IValidator<MeterReadingDto>, MeterReadingDtoValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

// CHeck for seed argument
var clargs = Environment.GetCommandLineArgs();
if (clargs.Length > 1 && clargs[1].ToLower() == "seed")
{
    // Seed Accounts
    var csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Test_Accounts.csv");
    await app.MigrateAndSeedDatabaseAsync(csvFilePath);
    return;
}

// Apply migrations and seeding
var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
if (!Directory.Exists(dataDirectory))
{
    Directory.CreateDirectory(dataDirectory);
}
var accountsCsvPath = Path.Combine(dataDirectory, "Test_Accounts.csv");
await app.MigrateAndSeedDatabaseAsync(accountsCsvPath);

app.Run();