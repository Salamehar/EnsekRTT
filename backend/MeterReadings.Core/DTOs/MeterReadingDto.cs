namespace MeterReadings.Core.DTOs;

public record MeterReadingDto
{
    public int AccountId { get; init; }
    public string MeterReadingDateTime { get; init; } = string.Empty;
    public string MeterReadValue { get; init; } = string.Empty;
}