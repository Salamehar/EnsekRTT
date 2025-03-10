namespace MeterReadings.Core.Models;

public class MeterReading
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public required DateTime MeterReadingDateTime { get; set; }
    public required int MeterReadValue { get; set; }
    public virtual Account? Account { get; set; }
}