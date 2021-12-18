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
        private readonly DatabaseSetupSignal _databaseSetupSignal;

        public BackgroundDatabaseMigrationService(
            IConfiguration configuration,
            ILogger<BackgroundDatabaseMigrationService> logger,
            DatabaseSetupSignal databaseSetupSignal)
        {
            _configuration = configuration;
            _logger = logger;
            _databaseSetupSignal = databaseSetupSignal;
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

                var connectionString = $"Server={server};Initial Catalog={database};{credentials}";
                var connection = new SqlConnection(connectionString);
                var databaseExists = false;
                try
                {
                    _logger.LogInformation($"Checking database {database} status");
                    _logger.LogInformation($"ConnectionString: {connectionString}");
                    await connection.OpenAsync();
                    databaseExists = connection.State == ConnectionState.Open;
                    await connection.CloseAsync();
                    _logger.LogInformation($"Database {database} exists");
                } 
                catch { }
                
                if(databaseExists == false)
                {
                    _logger.LogInformation($"Database {database} does not exist, creating...");
                    var connectionStringMaster = $"Server={server};Initial Catalog=master;{credentials}";
                    _logger.LogInformation($"ConnectionString: {connectionStringMaster}");
                    var connectionMaster = new SqlConnection(connectionStringMaster);
                    var createCommand = new SqlCommand($"CREATE DATABASE {database}", connectionMaster);
                    await connectionMaster.OpenAsync();
                    await createCommand.ExecuteNonQueryAsync();
                    if (connectionMaster.State == ConnectionState.Open)
                    {
                        await connectionMaster.CloseAsync();
                        await connection.OpenAsync();
                    }

                    RunScript(connection, "DBMasterTables.sql");
                    RunScript(connection, "DBMasterData.sql");
                }

                RunScript(connection, "DBMasterStoredProcedure.sql");

                _logger.LogInformation($"Database {database} migration finished");
                _databaseSetupSignal.SetDatabaseReady();
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected exception {e.Message} {e.StackTrace}", e);
            }
        }

        private void RunScript(SqlConnection connection, string script)
        {
            _logger.LogInformation($"Running {script} script: {connection.ConnectionString}");
            var currentExecutable = Assembly.GetExecutingAssembly().Location;
            var currentFolder = Path.GetDirectoryName(currentExecutable);
            var masterTables = File.ReadAllText($"{currentFolder}{Path.DirectorySeparatorChar}Master{Path.DirectorySeparatorChar}{script}");

            var server = new Server(new ServerConnection(connection));
            server.ConnectionContext.ExecuteNonQuery(masterTables);
        }
    }
}
