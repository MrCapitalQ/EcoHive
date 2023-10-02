using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Moq;
using MrCapitalQ.EcoHive.EcoBee.Auth;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore.Tests
{
    public class EcoBeeAuthCacheTests
    {
        private const string AuthTokenCacheKey = "EcoBeeAuthToken";
        private readonly Mock<IMemoryCache> _memoryCache;

        private readonly EcoBeeAuthCache _authCache;

        public EcoBeeAuthCacheTests()
        {
            _memoryCache = new();

            _authCache = new(_memoryCache.Object);
        }

        [Fact]
        public async Task GetAsync_NothingCachedOrExpired_ReturnsNull()
        {
            var actual = await _authCache.GetAsync();

            Assert.Null(actual);
        }

        [Fact]
        public async Task GetAsync_DataCached_ReturnsCachedData()
        {
            var data = new EcoBeeAuthTokenData
            {
                AccessToken = "fake_access_token",
                TokenType = "bearer"
            };
            object? cachedData = data;
            _memoryCache.Setup(c => c.TryGetValue(AuthTokenCacheKey, out cachedData)).Returns(true);

            var actual = await _authCache.GetAsync();

            Assert.Equal(data, actual);
        }

        [Fact]
        public void SetAsync_CachesWithExpiration()
        {
            var value = new EcoBeeAuthTokenData
            {
                AccessToken = "fake_access_token",
                TokenType = "bearer"
            };
            var expiration = TimeSpan.FromSeconds(10);
            TestCacheEntry cacheEntry = new(AuthTokenCacheKey);
            _memoryCache.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(cacheEntry);

            _authCache.SetAsync(value, expiration);

            Assert.Equal(value, cacheEntry.Value);
            Assert.Equal(expiration, cacheEntry.AbsoluteExpirationRelativeToNow);
            _memoryCache.Verify(c => c.CreateEntry(AuthTokenCacheKey));
        }

        private class TestCacheEntry : ICacheEntry
        {
            public TestCacheEntry(string key)
            {
                Key = key;
            }

            public DateTimeOffset? AbsoluteExpiration { get; set; }
            public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
            public IList<IChangeToken> ExpirationTokens => new List<IChangeToken>();
            public object Key { get; }
            public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks => new List<PostEvictionCallbackRegistration>();
            public CacheItemPriority Priority { get; set; }
            public long? Size { get; set; }
            public TimeSpan? SlidingExpiration { get; set; }
            public object? Value { get; set; }

            public void Dispose()
            { }
        }
    }
}
