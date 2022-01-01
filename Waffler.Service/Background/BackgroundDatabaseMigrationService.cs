using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

using Waffler.Service.Infrastructure;

namespace Waffler.Service.Background
{
    public class BackgroundDatabaseMigrationService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BackgroundDatabaseMigrationService> _logger;
        private readonly IDatabaseSetupSignal _databaseSetupSignal;

        public BackgroundDatabaseMigrationService(
            ILogger<BackgroundDatabaseMigrationService> logger,
            IConfiguration configuration,
            IDatabaseSetupSignal databaseSetupSignal)
        {
            _logger = logger;
            _configuration = configuration;
            _databaseSetupSignal = databaseSetupSignal;
            _logger.LogDebug("BackgroundDatabaseMigrationService instantiated");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await MigrateDatabase(cancellationToken);
        }

        private async Task MigrateDatabase(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Running database migration");
                var server = _configuration.GetValue<string>("Database:Server");
                var database = _configuration.GetValue<string>("Database:Catalog");
                var credentials = _configuration.GetValue<string>("Database:Credentials");

                var connectionStringMaster = $"Server={server};Initial Catalog=master;{credentials}";
                var connectionString = $"Server={server};Initial Catalog={database};{credentials}";

                await AwaitServerOnline(new SqlConnection(connectionStringMaster));
                var databaseExists = await DatabaseExists(new SqlConnection(connectionString));
                
                if(databaseExists == false && cancellationToken.IsCancellationRequested == false)
                {
                    await CreateDatabase(new SqlConnection(connectionStringMaster), database);

                    await AwaitServerOnline(new SqlConnection(connectionString));

                    await RunScript(new SqlConnection(connectionString), "DBMasterTables.sql");
                    await RunScript(new SqlConnection(connectionString), "DBMasterData.sql");
                }

                await RunScript (new SqlConnection(connectionString), "DBMasterStoredProcedure.sql");

                _logger.LogInformation($"Database {database} migration finished");
                _databaseSetupSignal.SetDatabaseReady();
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected exception {e.Message} {e.StackTrace}", e);
            }
        }

        private async Task CreateDatabase(SqlConnection connection, string database)
        {
            _logger.LogInformation($"Database {database} does not exist, creating...");
            _logger.LogInformation($"ConnectionString: {connection.ConnectionString}");

            var createCommand = new SqlCommand($"CREATE DATABASE {database}", connection);
            await connection.OpenAsync();
            await createCommand.ExecuteNonQueryAsync();
            await connection.CloseAsync();

            _logger.LogInformation($"Database {database} created");
        }

        private async Task<bool> DatabaseExists(SqlConnection connection)
        {
            try
            {
                _logger.LogInformation($"Checking database {connection.Database} status");
                _logger.LogInformation($"ConnectionString: {connection.ConnectionString}");
                await connection.OpenAsync();
                var databaseExists = connection.State == ConnectionState.Open;
                await connection.CloseAsync();

                return databaseExists;
            }
            catch { }

            return false;
        }

        private async Task AwaitServerOnline(SqlConnection connection)
        {
            while (true)
            {
                try
                {
                    await connection.OpenAsync();
                }
                catch (Exception e)
                {
                    _logger.LogDebug($"{e.Message} - {e.InnerException?.Message}");
                }

                if (connection.State != ConnectionState.Open)
                {
                    _logger.LogInformation($"Database {connection.Database} not online, waiting...");
                    Thread.Sleep(2000);
                }
                else
                {
                    _logger.LogInformation($"Database {connection.Database} online, proceeding with migration");
                    break;
                }
            }

            await connection.CloseAsync();
        }

        private async Task RunScript(SqlConnection connection, string script)
        {
            _logger.LogInformation($"Running {script} script on {connection.Database}");
            _logger.LogInformation($"ConnectionString: {connection.ConnectionString}");
            await connection.OpenAsync();

            var currentExecutable = Assembly.GetExecutingAssembly().Location;
            var currentFolder = Path.GetDirectoryName(currentExecutable);
            var masterTables = File.ReadAllText($"{currentFolder}{Path.DirectorySeparatorChar}Master{Path.DirectorySeparatorChar}{script}");

            var server = new Server(new ServerConnection(connection));
            server.ConnectionContext.ExecuteNonQuery(masterTables);

            await connection.CloseAsync();
        }
    }
}
