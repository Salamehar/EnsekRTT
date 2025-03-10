namespace MeterReadings.Core.DTOs;

public record MeterReadingUploadResultDto
{
    public int SuccessfulReadings { get; init; }
    public int FailedReadings { get; init; }
}