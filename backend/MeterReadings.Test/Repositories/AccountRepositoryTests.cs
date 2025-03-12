using MeterReadings.Core.Models;
using MeterReadings.Data.Repositories;
using MeterReadings.Test.Fixtures;
using MeterReadings.Test.Helpers;

namespace MeterReadings.Test.Repositories
{
    public class AccountRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;

        public AccountRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;

            ResetDatabase();
        }

        private void ResetDatabase()
        {
            // Clear existing data
            _fixture.DbContext.MeterReadings.RemoveRange(_fixture.DbContext.MeterReadings);
            _fixture.DbContext.Accounts.RemoveRange(_fixture.DbContext.Accounts);
            _fixture.DbContext.SaveChanges();

            // Add test accounts
            _fixture.DbContext.Accounts.AddRange(TestDataHelper.GetTestAccounts());
            _fixture.DbContext.SaveChanges();
        }

        [Fact]
        public async Task ExistsAsync_WithExistingAccountId_ReturnsTrue()
        {
            // Arrange
            var repository = new AccountRepository(_fixture.DbContext);

            // Act
            var exists = await repository.ExistsAsync(1);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingAccountId_ReturnsFalse()
        {
            // Arrange
            var repository = new AccountRepository(_fixture.DbContext);

            // Act
            var exists = await repository.ExistsAsync(999);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllAccounts()
        {
            // Arrange
            var repository = new AccountRepository(_fixture.DbContext);

            // Act
            var accounts = await repository.GetAllAsync();

            // Assert
            Assert.Equal(3, accounts.Count());
        }

        [Fact]
        public async Task SeedAccountsAsync_WithNewAccounts_AddsAccounts()
        {
            // Arrange
            var repository = new AccountRepository(_fixture.DbContext);
            var newAccounts = new[]
            {
                new Account { AccountId = 100, FirstName = "New", LastName = "User" }
            };

            // Act
            await repository.SeedAccountsAsync(newAccounts);

            // Assert
            var exists = await repository.ExistsAsync(100);
            Assert.True(exists);
        }

        [Fact]
        public async Task SeedAccountsAsync_WithExistingAccounts_SkipsExistingAccounts()
        {
            // Arrange
            var repository = new AccountRepository(_fixture.DbContext);
            var accounts = new[]
            {
                new Account { AccountId = 1, FirstName = "Modified", LastName = "User" }
            };

            // Initial state
            var initialAccount = await _fixture.DbContext.Accounts.FindAsync(1);
            Assert.NotNull(initialAccount);
            Assert.Equal("Clem", initialAccount.FirstName);

            // Act
            await repository.SeedAccountsAsync(accounts);

            // Assert - account should not be modified
            var account = await _fixture.DbContext.Accounts.FindAsync(1);
            Assert.NotNull(account);
            Assert.Equal("Clem", account.FirstName);
        }
    }
}