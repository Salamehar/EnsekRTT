using MeterReadings.Core.Models;

namespace MeterReadings.Core.Interfaces.Repositories;

public interface IAccountRepository
{
    Task<bool> ExistsAsync(int accountId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Account>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SeedAccountsAsync(IEnumerable<Account> accounts, CancellationToken cancellationToken = default);
}