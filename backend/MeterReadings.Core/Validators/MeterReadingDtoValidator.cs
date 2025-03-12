using System.Globalization;
using FluentValidation;
using MeterReadings.Core.DTOs;
using MeterReadings.Core.Interfaces.Repositories;

namespace MeterReadings.Core.Validators;

public class MeterReadingDtoValidator : AbstractValidator<MeterReadingDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMeterReadingRepository _meterReadingRepository;

    public MeterReadingDtoValidator(
        IAccountRepository accountRepository,
        IMeterReadingRepository meterReadingRepository)
    {
        _accountRepository = accountRepository;
        _meterReadingRepository = meterReadingRepository;

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .MustAsync(AccountExists)
            .WithMessage("Account ID does not exist.");

        RuleFor(x => x.MeterReadValue)
            .NotEmpty()
            .Matches(@"^\d{5}$")
            .WithMessage("Meter reading value must be in the format NNNNN (5 digits).");

        RuleFor(x => x)
            .MustAsync(ReadingNotDuplicate)
            .WithMessage("A reading with this account ID and date already exists.")
            .MustAsync(ReadingNotOlderThanExisting)
            .WithMessage("A newer reading already exists for this account.");
    }

    private async Task<bool> AccountExists(int accountId, CancellationToken cancellationToken)
    {
        return await _accountRepository.ExistsAsync(accountId, cancellationToken);
    }

    private async Task<bool> ReadingNotDuplicate(MeterReadingDto reading, CancellationToken cancellationToken)
    {
        if (!DateTime.TryParseExact(
              reading.MeterReadingDateTime,
              "dd/MM/yyyy HH:mm",
              CultureInfo.InvariantCulture,
              DateTimeStyles.None,
              out var readingDateTime))
        {
            return false;
        }

        return !await _meterReadingRepository.ExistsAsync(
            reading.AccountId,
            readingDateTime,
            cancellationToken);
    }

    private async Task<bool> ReadingNotOlderThanExisting(MeterReadingDto reading, CancellationToken cancellationToken)
    {
        if (!DateTime.TryParseExact(
              reading.MeterReadingDateTime,
              "dd/MM/yyyy HH:mm",
              CultureInfo.InvariantCulture,
              DateTimeStyles.None,
              out var readingDateTime))
        {
            return false;
        }

        var latestReadingDate = await _meterReadingRepository.GetLatestReadingDateTimeAsync(
            reading.AccountId,
            cancellationToken);

        return latestReadingDate == null || readingDateTime >= latestReadingDate;
    }
}