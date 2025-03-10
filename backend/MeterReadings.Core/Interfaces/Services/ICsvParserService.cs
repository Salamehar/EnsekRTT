namespace MeterReadings.Core.Interfaces.Services;

public interface ICsvParserService
{
    Task<IEnumerable<T>> ParseCsvAsync<T>(Stream csvStream, CancellationToken cancellationToken = default);
}