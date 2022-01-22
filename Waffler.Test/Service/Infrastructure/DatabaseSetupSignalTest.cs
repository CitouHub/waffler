using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Waffler.Service.Infrastructure;

namespace Waffler.Test.Service.Infrastructure
{
    public class DatabaseSetupSignalTest
    {
        private readonly DatabaseSetupSignal _databaseSetupSignal;

        public DatabaseSetupSignalTest()
        {
            var logger = Substitute.For<ILogger<DatabaseSetupSignal>>();
            _databaseSetupSignal = new DatabaseSetupSignal(logger);
        }

        [Fact]
        public async void IsDatabaseOnline_False()
        {
            //Setup
            var server = "Test";
            var database = "Test";
            var credentials = "persist security info=True;Integrated Security=SSPI";

            //Act
            var databaseOnline = await _databaseSetupSignal.IsDatabaseOnlineAsync(new SqlConnection($"Server={server};Initial Catalog={database};{credentials}"));

            //Assert
            Assert.False(databaseOnline);
        }
    }
}
