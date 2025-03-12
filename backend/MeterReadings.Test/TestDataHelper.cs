using System.Globalization;
using System.Text;
using MeterReadings.Core.DTOs;
using MeterReadings.Core.Models;

namespace MeterReadings.Test.Helpers
{
    public static class TestDataHelper
    {
        // private static readonly DateTime TestDateTime1 = new DateTime(2023, 4, 22, 12, 25, 0);
        // private static readonly DateTime TestDateTime2 = new DateTime(2023, 1, 1, 10, 0, 0);
        // private static readonly DateTime TestDateTime3 = new DateTime(2023, 4, 23, 15, 30, 0);
        public static DateTime GetTestDateTime1() => new DateTime(2023, 4, 22, 12, 25, 0);
        public static DateTime GetTestDateTime2() => new DateTime(2023, 1, 1, 10, 0, 0);
        public static DateTime GetTestDateTime3() => new DateTime(2023, 4, 23, 15, 30, 0);

        public static List<Account> GetTestAccounts()
        {
            return new List<Account>
            {
                new Account { AccountId = 1, FirstName = "Clem", LastName = "Evans" },
                new Account { AccountId = 2, FirstName = "Jane", LastName = "Smith" },
                new Account { AccountId = 3, FirstName = "Dylan", LastName = "Johnson" }
            };
        }

        public static List<MeterReading> GetTestMeterReadings()
        {
            return new List<MeterReading>
            {
                new MeterReading { Id = 1, AccountId = 1, MeterReadingDateTime = GetTestDateTime1(), MeterReadValue = 12345 },
                new MeterReading { Id = 2, AccountId = 2, MeterReadingDateTime = GetTestDateTime1(), MeterReadValue = 54321 }
            };
        }

        public static List<MeterReadingDto> GetValidMeterReadingDtos()
        {
            return new List<MeterReadingDto>
            {
                new MeterReadingDto { AccountId = 1, MeterReadingDateTime = GetTestDateTime1().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture), MeterReadValue = "12345" },
                new MeterReadingDto { AccountId = 2, MeterReadingDateTime = GetTestDateTime1().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture), MeterReadValue = "54321" }
            };
        }

        public static List<MeterReadingDto> GetInvalidMeterReadingDtos()
        {
            return new List<MeterReadingDto>
            {
                new MeterReadingDto { AccountId = 999, MeterReadingDateTime = GetTestDateTime1().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture), MeterReadValue = "12345" }, // Invalid account ID
                new MeterReadingDto { AccountId = 1, MeterReadingDateTime = GetTestDateTime1().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture), MeterReadValue = "1234" },    // Invalid meter read value
                new MeterReadingDto { AccountId = 1, MeterReadingDateTime = "invalid date", MeterReadValue = "12345" }, // Invalid date
                new MeterReadingDto { AccountId = 1, MeterReadingDateTime = GetTestDateTime1().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture), MeterReadValue = "abcde" }     // Non-numeric value
            };
        }

        public static Stream GenerateCsvStream(List<MeterReadingDto> readings)
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("AccountId,MeterReadingDateTime,MeterReadValue");

            foreach (var reading in readings)
            {
                csvBuilder.AppendLine($"{reading.AccountId},{reading.MeterReadingDateTime},{reading.MeterReadValue}");
            }

            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return new MemoryStream(bytes);
        }

        public static Stream GenerateInvalidCsvStream()
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("AccountId,MeterReadingDateTime"); //Missing column
            csvBuilder.AppendLine("1,22/04/2023 12:25,12345"); // Data doesn't match header

            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return new MemoryStream(bytes);
        }

        public static Stream GenerateEmptyCsvStream()
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("AccountId,MeterReadingDateTime,MeterReadValue");

            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return new MemoryStream(bytes);
        }

        public static string FormatDateForTests(DateTime date)
        {
            return date.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        }
    }
}