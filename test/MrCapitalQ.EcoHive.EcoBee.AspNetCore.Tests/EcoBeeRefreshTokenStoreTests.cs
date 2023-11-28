using Microsoft.EntityFrameworkCore;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore.Tests
{
    public sealed class EcoBeeRefreshTokenStoreTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
    {
        private readonly EcoBeeContext _dbContext;

        private readonly EcoBeeRefreshTokenStore _store;

        public EcoBeeRefreshTokenStoreTests(DatabaseFixture fixture)
        {
            var options = new DbContextOptionsBuilder<EcoBeeContext>()
                .UseSqlite(fixture.Connection)
                .Options;
            _dbContext = new(options);

            _store = new(_dbContext);
        }

        public Task InitializeAsync() => _dbContext.Database.EnsureCreatedAsync();

        public Task DisposeAsync() => _dbContext.Database.EnsureDeletedAsync();

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
    }
}