using MeterReadings.Core.Interfaces.Repositories;
using MeterReadings.Core.Models;
using MeterReadings.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly MeterReadingDbContext _context;

    public AccountRepository(MeterReadingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(int accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AnyAsync(a => a.AccountId == accountId, cancellationToken);
    }

    public async Task<IEnumerable<Account>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Accounts.ToListAsync(cancellationToken);
    }

    public async Task SeedAccountsAsync(IEnumerable<Account> accounts, CancellationToken cancellationToken = default)
    {
        // Skip already existing accounts
        var existingAccountIds = await _context.Accounts
            .Select(a => a.AccountId)
            .ToListAsync(cancellationToken);

        var newAccounts = accounts.Where(a => !existingAccountIds.Contains(a.AccountId));

        if (newAccounts.Any())
        {
            await _context.Accounts.AddRangeAsync(newAccounts, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}