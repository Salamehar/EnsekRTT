using MeterReadings.Core.DTOs;
using MeterReadings.Infrastructure.Services;
using MeterReadings.Test.Helpers;

namespace MeterReadings.Test.Services
{
    public class CsvParserServiceTests
    {
        [Fact]
        public async Task ParseCsvAsync_WithValidData_ReturnsCorrectRecords()
        {
            // Arrange
            var service = new CsvParserService();
            var validReadings = TestDataHelper.GetValidMeterReadingDtos();
            var csvStream = TestDataHelper.GenerateCsvStream(validReadings);

            // Act
            var result = await service.ParseCsvAsync<MeterReadingDto>(csvStream);

            // Assert
            Assert.Equal(2, result.Count());

            var readings = result.ToList();
            Assert.Equal(1, readings[0].AccountId);
            Assert.Equal("22/04/2023 12:25", readings[0].MeterReadingDateTime);
            Assert.Equal("12345", readings[0].MeterReadValue);

            Assert.Equal(2, readings[1].AccountId);
            Assert.Equal("22/04/2023 12:25", readings[1].MeterReadingDateTime);
            Assert.Equal("54321", readings[1].MeterReadValue);
        }

        [Fact]
        public async Task ParseCsvAsync_WithEmptyFile_ReturnsEmptyList()
        {
            // Arrange
            var service = new CsvParserService();
            var csvStream = TestDataHelper.GenerateEmptyCsvStream();

            // Act
            var result = await service.ParseCsvAsync<MeterReadingDto>(csvStream);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ParseCsvAsync_WithMalformedCsv_ReturnsAvailableData()
        {
            // Arrange
            var service = new CsvParserService();
            var csvStream = TestDataHelper.GenerateInvalidCsvStream();

            // Act
            var result = await service.ParseCsvAsync<MeterReadingDto>(csvStream);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ParseCsvAsync_WithMixedValidAndInvalidData_ParsesAllRecords()
        {
            // Arrange
            var service = new CsvParserService();
            var validReadings = TestDataHelper.GetValidMeterReadingDtos();
            var invalidReadings = TestDataHelper.GetInvalidMeterReadingDtos();
            var allReadings = validReadings.Concat(invalidReadings).ToList();
            var csvStream = TestDataHelper.GenerateCsvStream(allReadings);

            // Act
            var result = await service.ParseCsvAsync<MeterReadingDto>(csvStream);

            // Assert
            Assert.Equal(6, result.Count());
        }
    }
}