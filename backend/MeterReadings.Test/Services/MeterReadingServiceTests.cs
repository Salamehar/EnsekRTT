using FluentValidation;
using FluentValidation.Results;
using MeterReadings.Core.DTOs;
using MeterReadings.Core.Interfaces.Repositories;
using MeterReadings.Core.Interfaces.Services;
using MeterReadings.Core.Models;
using MeterReadings.Infrastructure.Services;
using MeterReadings.Test.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeterReadings.Test.Services
{
    public class MeterReadingServiceTests
    {
        private readonly Mock<ICsvParserService> _mockCsvParserService;
        private readonly Mock<IMeterReadingRepository> _mockMeterReadingRepository;
        private readonly Mock<IValidator<MeterReadingDto>> _mockValidator;
        private readonly Mock<ILogger<MeterReadingService>> _mockLogger;
        private readonly MeterReadingService _service;

        public MeterReadingServiceTests()
        {
            _mockCsvParserService = new Mock<ICsvParserService>();
            _mockMeterReadingRepository = new Mock<IMeterReadingRepository>();
            _mockValidator = new Mock<IValidator<MeterReadingDto>>();
            _mockLogger = new Mock<ILogger<MeterReadingService>>();

            _service = new MeterReadingService(
                _mockCsvParserService.Object,
                _mockMeterReadingRepository.Object,
                _mockValidator.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task ProcessMeterReadingsAsync_WithAllValidReadings_ReturnsSuccessResult()
        {
            // Arrange
            var validReadings = TestDataHelper.GetValidMeterReadingDtos();
            var csvStream = new MemoryStream();

            _mockCsvParserService
                .Setup(x => x.ParseCsvAsync<MeterReadingDto>(csvStream, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validReadings);

            _mockValidator
                .Setup(x => x.ValidateAsync(It.IsAny<MeterReadingDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _service.ProcessMeterReadingsAsync(csvStream);

            // Assert
            Assert.Equal(2, result.SuccessfulReadings);
            Assert.Equal(0, result.FailedReadings);

            _mockMeterReadingRepository.Verify(
                x => x.AddRangeAsync(It.Is<IEnumerable<MeterReading>>(readings => readings.Count() == 2),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessMeterReadingsAsync_WithAllInvalidReadings_ReturnsFailureResult()
        {
            // Arrange
            var invalidReadings = TestDataHelper.GetInvalidMeterReadingDtos();
            var csvStream = new MemoryStream();

            _mockCsvParserService
                .Setup(x => x.ParseCsvAsync<MeterReadingDto>(csvStream, It.IsAny<CancellationToken>()))
                .ReturnsAsync(invalidReadings);

            // Set up validation failures
            var validationFailure = new ValidationFailure("Property", "Error message");
            var validationResult = new ValidationResult(new[] { validationFailure });

            _mockValidator
                .Setup(x => x.ValidateAsync(It.IsAny<MeterReadingDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _service.ProcessMeterReadingsAsync(csvStream);

            // Assert
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.Equal(4, result.FailedReadings);

            _mockMeterReadingRepository.Verify(
                x => x.AddRangeAsync(It.IsAny<IEnumerable<MeterReading>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessMeterReadingsAsync_WithMixedValidInvalidReadings_ReturnsMixedResult()
        {
            // Arrange
            var validReadings = TestDataHelper.GetValidMeterReadingDtos();
            var invalidReadings = TestDataHelper.GetInvalidMeterReadingDtos();
            var allReadings = validReadings.Concat(invalidReadings).ToList();
            var csvStream = new MemoryStream();

            _mockCsvParserService
                .Setup(x => x.ParseCsvAsync<MeterReadingDto>(csvStream, It.IsAny<CancellationToken>()))
                .ReturnsAsync(allReadings);

            // Set up validation to succeed for valid readings and fail for invalid
            _mockValidator
                .Setup(x => x.ValidateAsync(It.Is<MeterReadingDto>(dto =>
                    dto.AccountId <= 2 && dto.MeterReadValue.Length == 5), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockValidator
                .Setup(x => x.ValidateAsync(It.Is<MeterReadingDto>(dto =>
                    dto.AccountId > 2 || dto.MeterReadValue.Length != 5), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Property", "Error") }));

            // Act
            var result = await _service.ProcessMeterReadingsAsync(csvStream);

            // Assert
            Assert.Equal(2, result.SuccessfulReadings);
            Assert.Equal(4, result.FailedReadings);

            _mockMeterReadingRepository.Verify(
                x => x.AddRangeAsync(It.Is<IEnumerable<MeterReading>>(readings => readings.Count() == 2),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessMeterReadingsAsync_WithInvalidDateFormat_CountsAsFailed()
        {
            // Arrange
            var readings = new List<MeterReadingDto>
            {
                new MeterReadingDto { AccountId = 1, MeterReadingDateTime = "invalid-date", MeterReadValue = "12345" }
            };
            var csvStream = new MemoryStream();

            _mockCsvParserService
                .Setup(x => x.ParseCsvAsync<MeterReadingDto>(csvStream, It.IsAny<CancellationToken>()))
                .ReturnsAsync(readings);

            _mockValidator
                .Setup(x => x.ValidateAsync(It.IsAny<MeterReadingDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _service.ProcessMeterReadingsAsync(csvStream);

            // Assert
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.Equal(1, result.FailedReadings);
        }

        [Fact]
        public async Task ProcessMeterReadingsAsync_WithInvalidMeterReadValue_CountsAsFailed()
        {
            // Arrange
            var readings = new List<MeterReadingDto>
            {
                new MeterReadingDto { AccountId = 1, MeterReadingDateTime = "22/04/2023 12:25", MeterReadValue = "abc" }
            };
            var csvStream = new MemoryStream();

            _mockCsvParserService
                .Setup(x => x.ParseCsvAsync<MeterReadingDto>(csvStream, It.IsAny<CancellationToken>()))
                .ReturnsAsync(readings);

            _mockValidator
                .Setup(x => x.ValidateAsync(It.IsAny<MeterReadingDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _service.ProcessMeterReadingsAsync(csvStream);

            // Assert
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.Equal(1, result.FailedReadings);
        }
    }
}