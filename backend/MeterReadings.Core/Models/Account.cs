namespace MeterReadings.Core.Models;

public class Account
{
    public int AccountId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public virtual ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
}