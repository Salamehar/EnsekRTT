using System.Globalization;
using FluentValidation;
using MeterReadings.Core.DTOs;
using MeterReadings.Core.Interfaces.Repositories;
using MeterReadings.Core.Interfaces.Services;
using MeterReadings.Core.Validators;
using MeterReadings.Data.Context;
using MeterReadings.Data.Repositories;
using MeterReadings.Infrastructure.Services;
using MeterReadings.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeterReadings.Test.Integration
{
    public class MeterReadingIntegrationTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly MeterReadingDbContext _dbContext;

        public MeterReadingIntegrationTests()
        {
            var services = new ServiceCollection();

            var dbContextOptions = new DbContextOptionsBuilder<MeterReadingDbContext>()
                .UseInMemoryDatabase($"MeterReadings_{Guid.NewGuid()}")
                .Options;

            _dbContext = new MeterReadingDbContext(dbContextOptions);
            services.AddSingleton(_dbContext);

            // Add repositories
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();

            // Add services
            services.AddScoped<ICsvParserService, CsvParserService>();
            services.AddScoped<IMeterReadingService, MeterReadingService>();

            // Add validator
            services.AddScoped<IValidator<MeterReadingDto>, MeterReadingDtoValidator>();

            // Add logger
            services.AddLogging(builder => builder.AddConsole());

            _serviceProvider = services.BuildServiceProvider();

            SeedTestData();
        }

        private void SeedTestData()
        {
            var accounts = TestDataHelper.GetTestAccounts();
            _dbContext.Accounts.AddRange(accounts);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task ProcessMeterReadings_EndToEnd_ProcessesReadingsCorrectly()
        {
            // Arrange
            var meterReadingService = _serviceProvider.GetRequiredService<IMeterReadingService>();
            var validReadings = TestDataHelper.GetValidMeterReadingDtos();
            var csvStream = TestDataHelper.GenerateCsvStream(validReadings);

            // Act
            var result = await meterReadingService.ProcessMeterReadingsAsync(csvStream);

            // Assert
            Assert.Equal(2, result.SuccessfulReadings);
            Assert.Equal(0, result.FailedReadings);

            // Verify readings were saved to database
            var meterReadings = _dbContext.MeterReadings.ToList();
            Assert.Equal(2, meterReadings.Count);

            // Verify first reading values
            var reading1 = meterReadings.FirstOrDefault(r => r.AccountId == 1);
            Assert.NotNull(reading1);
            Assert.Equal(12345, reading1.MeterReadValue);
            Assert.Equal(DateTime.ParseExact("22/04/2023 12:25", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture), reading1.MeterReadingDateTime);

            // Verify second reading values
            var reading2 = meterReadings.FirstOrDefault(r => r.AccountId == 2);
            Assert.NotNull(reading2);
            Assert.Equal(54321, reading2.MeterReadValue);
        }

        [Fact]
        public async Task ProcessMeterReadings_WithInvalidReadings_RejectsThem()
        {
            // Arrange
            var meterReadingService = _serviceProvider.GetRequiredService<IMeterReadingService>();
            var invalidReadings = TestDataHelper.GetInvalidMeterReadingDtos();
            var csvStream = TestDataHelper.GenerateCsvStream(invalidReadings);

            // Act
            var result = await meterReadingService.ProcessMeterReadingsAsync(csvStream);

            // Assert
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.Equal(4, result.FailedReadings);

            // Verify no readings were saved to database
            var meterReadings = _dbContext.MeterReadings.ToList();
            Assert.Empty(meterReadings);
        }

        [Fact]
        public async Task ProcessMeterReadings_WithMixedReadings_ProcessesValidOnes()
        {
            // Arrange
            var meterReadingService = _serviceProvider.GetRequiredService<IMeterReadingService>();
            var validReadings = TestDataHelper.GetValidMeterReadingDtos();
            var invalidReadings = TestDataHelper.GetInvalidMeterReadingDtos();
            var allReadings = validReadings.Concat(invalidReadings).ToList();
            var csvStream = TestDataHelper.GenerateCsvStream(allReadings);

            // Act
            var result = await meterReadingService.ProcessMeterReadingsAsync(csvStream);

            // Assert
            Assert.Equal(2, result.SuccessfulReadings);
            Assert.Equal(4, result.FailedReadings);

            // Verify only valid readings were saved to database
            var meterReadings = _dbContext.MeterReadings.ToList();
            Assert.Equal(2, meterReadings.Count);

            // Check account IDs of saved readings
            var accountIds = meterReadings.Select(r => r.AccountId).ToList();
            Assert.Contains(1, accountIds);
            Assert.Contains(2, accountIds);

            // Verify invalid account (999) was not saved
            Assert.DoesNotContain(999, accountIds);
        }

        [Fact]
        public async Task ProcessMeterReadings_WithDuplicateReadings_RejectsDuplicates()
        {
            // Arrange
            var meterReadingService = _serviceProvider.GetRequiredService<IMeterReadingService>();

            // First upload
            var validReadings = TestDataHelper.GetValidMeterReadingDtos();
            var firstCsvStream = TestDataHelper.GenerateCsvStream(validReadings);
            await meterReadingService.ProcessMeterReadingsAsync(firstCsvStream);

            // Second upload with same data
            var secondCsvStream = TestDataHelper.GenerateCsvStream(validReadings);

            // Act
            var result = await meterReadingService.ProcessMeterReadingsAsync(secondCsvStream);

            // Assert
            Assert.Equal(0, result.SuccessfulReadings);
            Assert.Equal(2, result.FailedReadings);

            // Verify there are only 2 readings in the database
            var meterReadings = _dbContext.MeterReadings.ToList();
            Assert.Equal(2, meterReadings.Count);
        }

        [Fact]
        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            _serviceProvider.Dispose();
        }
    }
}