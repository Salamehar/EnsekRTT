using CsvHelper;
using MeterReadings.Core.Models;
using MeterReadings.Data.Context;
using System.Globalization;

namespace MeterReadings.Data.Seed;

public class Seeder
{
    private readonly MeterReadingDbContext _context;

    public Seeder(MeterReadingDbContext context)
    {
        _context = context;
    }

    public async Task SeedAccountsFromCsvAsync(string csvFilePath)
    {
        if (!File.Exists(csvFilePath))
        {
            throw new FileNotFoundException("Accounts CSV file not found", csvFilePath);
        }

        using var reader = new StreamReader(csvFilePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var records = csv.GetRecords<Account>().ToList();

        if (!_context.Accounts.Any())
        {
            await _context.Accounts.AddRangeAsync(records);
            await _context.SaveChangesAsync();
        }
    }
}