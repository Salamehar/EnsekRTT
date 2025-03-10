using MeterReadings.Core.DTOs;

namespace MeterReadings.Core.Interfaces.Services;

public interface IMeterReadingService
{
    Task<MeterReadingUploadResultDto> ProcessMeterReadingsAsync(Stream csvStream, CancellationToken cancellationToken = default);
}