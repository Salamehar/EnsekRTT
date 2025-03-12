using MeterReadings.Core.Models;
using MeterReadings.Data.Repositories;
using MeterReadings.Test.Fixtures;
using MeterReadings.Test.Helpers;

namespace MeterReadings.Test.Repositories
{
    public class MeterReadingRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public MeterReadingRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ExistsAsync_WithExistingReadingAndDate_ReturnsTrue()
        {
            // Arrange
            var repository = new MeterReadingRepository(_fixture.DbContext);
            var readingDateTime = TestDataHelper.GetTestDateTime1();

            // Act
            var exists = await repository.ExistsAsync(1, readingDateTime);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingReading_ReturnsFalse()
        {
            // Arrange
            var repository = new MeterReadingRepository(_fixture.DbContext);
            var readingDateTime = TestDataHelper.GetTestDateTime2();

            // Act
            var exists = await repository.ExistsAsync(1, readingDateTime);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task GetLatestReadingDateTimeAsync_WithExistingReadings_ReturnsLatestDate()
        {
            // Arrange
            var repository = new MeterReadingRepository(_fixture.DbContext);
            var expectedDate = TestDataHelper.GetTestDateTime1();

            // Act
            var latestDate = await repository.GetLatestReadingDateTimeAsync(1);

            // Assert
            Assert.Equal(expectedDate, latestDate);
        }

        [Fact]
        public async Task GetLatestReadingDateTimeAsync_WithNoReadings_ReturnsNull()
        {
            // Arrange
            var repository = new MeterReadingRepository(_fixture.DbContext);

            // Act
            var latestDate = await repository.GetLatestReadingDateTimeAsync(999);

            // Assert
            Assert.Null(latestDate);
        }

        [Fact]
        public async Task AddRangeAsync_AddsReadingsToDatabase()
        {
            // Arrange
            var repository = new MeterReadingRepository(_fixture.DbContext);
            var newReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    AccountId = 3,
                    MeterReadingDateTime = TestDataHelper.GetTestDateTime3(),
                    MeterReadValue = 98765
                }
            };

            // Act
            await repository.AddRangeAsync(newReadings);

            // Assert
            var exists = await repository.ExistsAsync(3, TestDataHelper.GetTestDateTime3());

            Assert.True(exists);

            // Verify reading value
            var reading = _fixture.DbContext.MeterReadings
                .FirstOrDefault(m => m.AccountId == 3
                                   && m.MeterReadingDateTime == TestDataHelper.GetTestDateTime3());

            Assert.NotNull(reading);
            Assert.Equal(98765, reading.MeterReadValue);
        }
    }
}