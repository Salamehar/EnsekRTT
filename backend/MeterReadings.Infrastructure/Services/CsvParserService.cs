using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MeterReadings.Core.Interfaces.Services;

namespace MeterReadings.Infrastructure.Services;

public class CsvParserService : ICsvParserService
{
    public async Task<IEnumerable<T>> ParseCsvAsync<T>(Stream csvStream, CancellationToken cancellationToken = default)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, config);

        var records = new List<T>();
        await foreach (var record in csv.GetRecordsAsync<T>())
        {
            records.Add(record);
        }

        return records;
    }
}