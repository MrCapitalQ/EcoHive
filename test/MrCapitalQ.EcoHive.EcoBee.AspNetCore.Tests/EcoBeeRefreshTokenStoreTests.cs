using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore.Tests
{
    public sealed class EcoBeeRefreshTokenStoreTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly EcoBeeContext _dbContext;

        private readonly EcoBeeRefreshTokenStore _store;

        public EcoBeeRefreshTokenStoreTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<EcoBeeContext>()
                .UseSqlite(_connection)
                .Options;
            _dbContext = new EcoBeeContext(options);
            _dbContext.Database.EnsureCreated();

            _store = new EcoBeeRefreshTokenStore(_dbContext);
        }

        [Fact]
        public async Task GetAsync_NoRefreshTokenEntries_ReturnsNull()
        {
            var actual = await _store.GetAsync();

            Assert.Null(actual);
        }

        [Fact]
        public async Task GetAsync_WithExistingEntry_ReturnsCached()
        {
            var expected = "fake_refresh_token";
            _dbContext.RefreshTokens.Add(new()
            {
                Token = expected
            });
            await _dbContext.SaveChangesAsync();
            _dbContext.ChangeTracker.Clear();

            var actual = await _store.GetAsync();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task SetAsync_WithValue_SavesEntry()
        {
            var refreshToken = "fake_refresh_token";
            var expected = new RefreshToken()
            {
                Id = 1,
                Token = refreshToken
            };

            await _store.SetAsync(refreshToken);

            var record = await _dbContext.RefreshTokens.SingleAsync();
            Assert.Equal(expected.Id, record.Id);
            Assert.Equal(expected.Token, record.Token);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}