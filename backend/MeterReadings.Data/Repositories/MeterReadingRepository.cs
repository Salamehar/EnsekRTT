using MeterReadings.Core.Interfaces.Repositories;
using MeterReadings.Core.Models;
using MeterReadings.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Data.Repositories;

public class MeterReadingRepository : IMeterReadingRepository
{
    private readonly MeterReadingDbContext _context;

    public MeterReadingRepository(MeterReadingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(int accountId, DateTime readingDateTime, CancellationToken cancellationToken = default)
    {
        return await _context.MeterReadings
            .AnyAsync(m => m.AccountId == accountId && m.MeterReadingDateTime == readingDateTime, cancellationToken);
    }

    public async Task<DateTime?> GetLatestReadingDateTimeAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await _context.MeterReadings
            .Where(m => m.AccountId == accountId)
            .OrderByDescending(m => m.MeterReadingDateTime)
            .Select(m => (DateTime?)m.MeterReadingDateTime)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<MeterReading> meterReadings, CancellationToken cancellationToken = default)
    {
        await _context.MeterReadings.AddRangeAsync(meterReadings, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}