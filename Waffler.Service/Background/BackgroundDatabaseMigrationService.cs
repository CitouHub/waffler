using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Linq;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using Waffler.Service.Infrastructure;
using Waffler.Data;

namespace Waffler.Service.Background
{
    public class BackgroundDatabaseMigrationService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundDatabaseMigrationService> _logger;
        private readonly IDatabaseSetupSignal _databaseSetupSignal;

        private readonly string ScriptSectionMaster = "Master";
        private readonly string ScriptSectionMigration = "Migration";

        public BackgroundDatabaseMigrationService(
            ILogger<BackgroundDatabaseMigrationService> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IDatabaseSetupSignal databaseSetupSignal)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _databaseSetupSignal = databaseSetupSignal;
            _logger.LogDebug("Instantiated");
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

                _logger.LogInformation($"Waiting for master database to come online");
                await _databaseSetupSignal.AwaitDatabaseOnlineAsync(cancellationToken, new SqlConnection(connectionStringMaster));

                var databaseExists = await DatabaseExists(new SqlConnection(connectionString));
                
                if(databaseExists == false && cancellationToken.IsCancellationRequested == false)
                {
                    await CreateDatabase(new SqlConnection(connectionStringMaster), database);

                    _logger.LogInformation($"Waiting for database to come online");
                    await _databaseSetupSignal.AwaitDatabaseOnlineAsync(cancellationToken, new SqlConnection(connectionString));

                    await RunScript(new SqlConnection(connectionString), ScriptSectionMaster, "DBMasterTables.sql");
                    await RunScript(new SqlConnection(connectionString), ScriptSectionMaster, "DBMasterData.sql");
                }

                await RunScript (new SqlConnection(connectionString), ScriptSectionMaster, "DBMasterStoredProcedure.sql");

                await RunMigrationScripts(connectionString);

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
                await connection.OpenAsync();
                var databaseExists = connection.State == ConnectionState.Open;
                await connection.CloseAsync();

                return databaseExists;
            }
            catch { }

            return false;
        }

        private async Task RunScript(SqlConnection connection, string section, string script)
        {
            _logger.LogInformation($"Running {script} script on {connection.Database}");
            await connection.OpenAsync();

            var currentExecutable = Assembly.GetExecutingAssembly().Location;
            var currentFolder = Path.GetDirectoryName(currentExecutable);
            var scriptFile = File.ReadAllText($"{currentFolder}{Path.DirectorySeparatorChar}{section}{Path.DirectorySeparatorChar}{script}");

            var server = new Server(new ServerConnection(connection));
            server.ConnectionContext.ExecuteNonQuery(scriptFile);

            await connection.CloseAsync();
        }

        private async Task RunMigrationScripts(string connectionString)
        {
            _logger.LogInformation($"Running database migration scripts");
            var currentExecutable = Assembly.GetExecutingAssembly().Location;
            var currentFolder = Path.GetDirectoryName(currentExecutable);
            var migrationScripts = Directory.GetFiles($"{currentFolder}{Path.DirectorySeparatorChar}{ScriptSectionMigration}{Path.DirectorySeparatorChar}")
                .Select(_ => new FileInfo(_)).ToList();
            _logger.LogInformation($"Database migration contains {migrationScripts.Count} scripts");

            using IServiceScope scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WafflerDbContext>();
            var executedScripts = await context.DatabaseMigrations.Select(_ => _.ScriptName).ToListAsync();
            var newScripts = migrationScripts.Where(n => executedScripts.Any(e => e == n.Name) == false).ToList();
            _logger.LogInformation($"{newScripts.Count} new migration scripts found");

            foreach (var script in newScripts)
            {
                await RunScript(new SqlConnection(connectionString), ScriptSectionMigration, script.Name);
                context.DatabaseMigrations.Add(new DatabaseMigration()
                {
                    InsertByUser = 1,
                    InsertDate = DateTime.UtcNow,
                    ScriptName = script.Name
                });
                context.SaveChanges();
            }
        }
    }
}
