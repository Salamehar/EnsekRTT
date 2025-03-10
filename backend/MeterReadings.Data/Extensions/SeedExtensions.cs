using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MeterReadings.Core.Models;
using MeterReadings.Data.Context;
using CsvHelper;
using System.Globalization;

namespace MeterReadings.Data.Extensions;

public static class SeedExtensions
{
    public static async Task MigrateAndSeedDatabaseAsync(this IApplicationBuilder app, string csvFilePath)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MeterReadingDbContext>();

        // Apply migrations
        await dbContext.Database.MigrateAsync();

        // Seed accounts if none exist
        if (!await dbContext.Accounts.AnyAsync())
        {
            await SeedAccountsFromCsvAsync(dbContext, csvFilePath);
        }
    }

    private static async Task SeedAccountsFromCsvAsync(MeterReadingDbContext dbContext, string csvFilePath)
    {
        if (!File.Exists(csvFilePath))
        {
            throw new FileNotFoundException("Accounts CSV file not found", csvFilePath);
        }

        using var reader = new StreamReader(csvFilePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<Account>().ToList();

        await dbContext.Accounts.AddRangeAsync(records);
        await dbContext.SaveChangesAsync();
    }
}