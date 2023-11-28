using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace MrCapitalQ.EcoHive.EcoBee.AspNetCore.Tests
{
    public sealed class DatabaseFixture : IAsyncLifetime
    {
        private readonly DbConnection _connection;

        public DatabaseFixture() => _connection = new SqliteConnection("Filename=:memory:");

        public DbConnection Connection => _connection;

        public Task InitializeAsync() => _connection.OpenAsync();

        public Task DisposeAsync() => _connection.DisposeAsync().AsTask();
    }
}
