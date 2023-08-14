using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MrCapitalQ.EcoHive.EcoBee.Auth;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore.Tests
{
    public sealed class EcoBeeAuthCacheTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly EcoBeeCacheContext _dbContext;

        private readonly EcoBeeAuthCache _cache;

        public EcoBeeAuthCacheTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();
            var options = new DbContextOptionsBuilder<EcoBeeCacheContext>()
                .UseSqlite(_connection)
                .Options;
            _dbContext = new EcoBeeCacheContext(options);
            _dbContext.Database.EnsureCreated();

            _cache = new EcoBeeAuthCache(_dbContext);
        }

        [Fact]
        public async Task GetAsync_NoAuthTokenEntries_ReturnsNull()
        {
            var actual = await _cache.GetAysnc();

            Assert.Null(actual);
        }

        [Fact]
        public async Task GetAsync_WithAuthTokenEntry_ReturnsCached()
        {
            var expected = new EcoBeeAuthTokenData()
            {
                AccessToken = "fake_access_token",
                RefreshToken = "fake_refresh_token",
                Expiration = DateTimeOffset.UtcNow
            };
            _dbContext.AuthTokens.Add(new()
            {
                AccessToken = expected.AccessToken,
                RefreshToken = expected.RefreshToken,
                Expiration = expected.Expiration
            });
            await _dbContext.SaveChangesAsync();
            _dbContext.ChangeTracker.Clear();

            var actual = await _cache.GetAysnc();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task SetAsync_WithExistingAuthTokenEntry_ClearsExisting()
        {
            var existing = new EcoBeeAuthTokenData()
            {
                AccessToken = "fake_access_token",
                RefreshToken = "fake_refresh_token",
                Expiration = DateTimeOffset.UtcNow
            };
            _dbContext.AuthTokens.Add(new()
            {
                AccessToken = existing.AccessToken,
                RefreshToken = existing.RefreshToken,
                Expiration = existing.Expiration
            });
            await _dbContext.SaveChangesAsync();
            _dbContext.ChangeTracker.Clear();

            await _cache.SetAysnc(null);

            Assert.False(await _dbContext.AuthTokens.AnyAsync());
        }

        [Fact]
        public async Task SetAsync_NonNullValue_SavesEntry()
        {
            var data = new EcoBeeAuthTokenData()
            {
                AccessToken = "fake_access_token",
                RefreshToken = "fake_refresh_token",
                Expiration = DateTimeOffset.UtcNow
            };
            var expected = new AuthToken()
            {
                Id = 1,
                AccessToken = data.AccessToken,
                RefreshToken = data.RefreshToken,
                Expiration = data.Expiration
            };

            await _cache.SetAysnc(data);

            var record = await _dbContext.AuthTokens.SingleAsync();
            Assert.Equal(expected, record);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}