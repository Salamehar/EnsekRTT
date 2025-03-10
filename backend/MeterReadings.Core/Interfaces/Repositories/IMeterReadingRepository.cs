using MeterReadings.Core.Models;

namespace MeterReadings.Core.Interfaces.Repositories;

public interface IMeterReadingRepository
{
    Task<bool> ExistsAsync(int accountId, DateTime readingDateTime, CancellationToken cancellationToken = default);
    Task<DateTime?> GetLatestReadingDateTimeAsync(int accountId, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<MeterReading> meterReadings, CancellationToken cancellationToken = default);
}