using MeterReadings.Data.Context;
using MeterReadings.Test.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MeterReadings.Test.Fixtures
{
    public class DatabaseFixture : IDisposable
    {
        public MeterReadingDbContext DbContext { get; }

        public DatabaseFixture()
        {
            var options = new DbContextOptionsBuilder<MeterReadingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            DbContext = new MeterReadingDbContext(options);
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            DbContext.Accounts.AddRange(TestDataHelper.GetTestAccounts());

            DbContext.MeterReadings.AddRange(TestDataHelper.GetTestMeterReadings());

            DbContext.SaveChanges();
        }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
        }
    }
}