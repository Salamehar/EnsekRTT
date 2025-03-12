using MeterReadings.Core.DTOs;
using MeterReadings.Core.Interfaces.Repositories;
using MeterReadings.Core.Validators;
using MeterReadings.Test.Helpers;
using Moq;

namespace MeterReadings.Test.Validators
{
    public class MeterReadingDtoValidatorTests
    {
        private readonly Mock<IAccountRepository> _mockAccountRepository;
        private readonly Mock<IMeterReadingRepository> _mockMeterReadingRepository;
        private readonly MeterReadingDtoValidator _validator;

        public MeterReadingDtoValidatorTests()
        {
            _mockAccountRepository = new Mock<IAccountRepository>();
            _mockMeterReadingRepository = new Mock<IMeterReadingRepository>();
            _validator = new MeterReadingDtoValidator(
                _mockAccountRepository.Object,
                _mockMeterReadingRepository.Object
            );
        }

        [Fact]
        public async Task Validate_WithValidReading_Succeeds()
        {
            // Arrange
            var dto = new MeterReadingDto
            {
                AccountId = 1,
                MeterReadingDateTime = TestDataHelper.FormatDateForTests(TestDataHelper.GetTestDateTime1()),
                MeterReadValue = "12345"
            };

            _mockAccountRepository
                .Setup(x => x.ExistsAsync(dto.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMeterReadingRepository
                .Setup(x => x.ExistsAsync(dto.AccountId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockMeterReadingRepository
                .Setup(x => x.GetLatestReadingDateTimeAsync(dto.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DateTime?)null);

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task Validate_WithInvalidAccountId_Fails()
        {
            // Arrange
            var dto = new MeterReadingDto
            {
                AccountId = 0, // Invalid account ID
                MeterReadingDateTime = TestDataHelper.FormatDateForTests(TestDataHelper.GetTestDateTime1()),
                MeterReadValue = "12345"
            };

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "AccountId");
        }

        [Fact]
        public async Task Validate_WithNonExistentAccount_Fails()
        {
            // Arrange
            var dto = new MeterReadingDto
            {
                AccountId = 999,
                MeterReadingDateTime = TestDataHelper.FormatDateForTests(TestDataHelper.GetTestDateTime1()),
                MeterReadValue = "12345"
            };

            _mockAccountRepository
                .Setup(x => x.ExistsAsync(dto.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "AccountId");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Account ID does not exist.");
        }

        [Fact]
        public async Task Validate_WithInvalidMeterReadingFormat_Fails()
        {
            // Arrange
            var dto = new MeterReadingDto
            {
                AccountId = 1,
                MeterReadingDateTime = TestDataHelper.FormatDateForTests(TestDataHelper.GetTestDateTime1()),
                MeterReadValue = "123"
            };

            _mockAccountRepository
                .Setup(x => x.ExistsAsync(dto.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "MeterReadValue");
        }

        [Fact]
        public async Task Validate_WithDuplicateReading_Fails()
        {
            // Arrange
            var dto = new MeterReadingDto
            {
                AccountId = 1,
                MeterReadingDateTime = TestDataHelper.FormatDateForTests(TestDataHelper.GetTestDateTime1()),
                MeterReadValue = "12345"
            };

            _mockAccountRepository
                .Setup(x => x.ExistsAsync(dto.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMeterReadingRepository
                .Setup(x => x.ExistsAsync(dto.AccountId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "A reading with this account ID and date already exists.");
        }

        [Fact]
        public async Task Validate_WithOlderThanExistingReading_Fails()
        {
            // Arrange
            var dto = new MeterReadingDto
            {
                AccountId = 1,
                MeterReadingDateTime = TestDataHelper.FormatDateForTests(TestDataHelper.GetTestDateTime1()),
                MeterReadValue = "12345"
            };

            _mockAccountRepository
                .Setup(x => x.ExistsAsync(dto.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMeterReadingRepository
                .Setup(x => x.ExistsAsync(dto.AccountId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Set up a newer reading date
            var newerDate = TestDataHelper.GetTestDateTime3(); // 23 April, later than the test date (22nd April)
            _mockMeterReadingRepository
                .Setup(x => x.GetLatestReadingDateTimeAsync(dto.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(newerDate);

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "A newer reading already exists for this account.");
        }

        [Fact]
        public async Task Validate_WithInvalidDateFormat_FailsValidation()
        {
            // Arrange
            var dto = new MeterReadingDto
            {
                AccountId = 1,
                MeterReadingDateTime = "invalid-date",
                MeterReadValue = "12345"
            };

            _mockAccountRepository
                .Setup(x => x.ExistsAsync(dto.AccountId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _validator.ValidateAsync(dto);

            // Assert
            Assert.False(result.IsValid);
        }
    }
}