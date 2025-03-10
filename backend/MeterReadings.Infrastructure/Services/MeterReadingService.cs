using FluentValidation;
using MeterReadings.Core.DTOs;
using MeterReadings.Core.Interfaces.Repositories;
using MeterReadings.Core.Interfaces.Services;
using MeterReadings.Core.Models;

namespace MeterReadings.Infrastructure.Services;

public class MeterReadingService : IMeterReadingService
{
    private readonly ICsvParserService _csvParserService;
    private readonly IMeterReadingRepository _meterReadingRepository;
    private readonly IValidator<MeterReadingDto> _validator;

    public MeterReadingService(
        ICsvParserService csvParserService,
        IMeterReadingRepository meterReadingRepository,
        IValidator<MeterReadingDto> validator)
    {
        _csvParserService = csvParserService;
        _meterReadingRepository = meterReadingRepository;
        _validator = validator;
    }

    public async Task<MeterReadingUploadResultDto> ProcessMeterReadingsAsync(
        Stream csvStream,
        CancellationToken cancellationToken = default)
    {
        // Reset to the beginning of the csv stream
        csvStream.Position = 0;

        // Parse CSV file
        var meterReadingDtos = await _csvParserService.ParseCsvAsync<MeterReadingDto>(csvStream, cancellationToken);

        var successfulReadings = new List<MeterReading>();
        var failedReadings = 0;

        // Validate readings
        foreach (var dto in meterReadingDtos)
        {
            var validationResult = await _validator.ValidateAsync(dto, cancellationToken);

            if (validationResult.IsValid &&
                DateTime.TryParse(dto.MeterReadingDateTime, out var readingDateTime) &&
                int.TryParse(dto.MeterReadValue, out var readingValue))
            {
                successfulReadings.Add(new MeterReading
                {
                    AccountId = dto.AccountId,
                    MeterReadingDateTime = readingDateTime,
                    MeterReadValue = readingValue
                });
            }
            else
            {
                failedReadings++;
            }
        }

        // Save valid readings to db
        if (successfulReadings.Any())
        {
            await _meterReadingRepository.AddRangeAsync(successfulReadings, cancellationToken);
        }

        return new MeterReadingUploadResultDto
        {
            SuccessfulReadings = successfulReadings.Count,
            FailedReadings = failedReadings
        };
    }
}