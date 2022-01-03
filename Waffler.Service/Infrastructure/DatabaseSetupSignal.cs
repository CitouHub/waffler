using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Waffler.Service.Infrastructure
{
    public interface IDatabaseSetupSignal
    {
        Task AwaitDatabaseReadyAsync(CancellationToken cancellationToken);
        void SetDatabaseReady();
        void SetDatabaseBusy();
        Task AwaitDatabaseOnlineAsync(SqlConnection connection);
    }

    public class DatabaseSetupSignal : IDatabaseSetupSignal
    {
        private readonly SemaphoreSlim _databaseReadySignal;
        private readonly ILogger<DatabaseSetupSignal> _logger;
        private readonly object Lock = new object();

        private bool DatabaseReady;
        private short Waiting;

        public DatabaseSetupSignal(ILogger<DatabaseSetupSignal> logger)
        {
            _databaseReadySignal = new SemaphoreSlim(0);
            _logger = logger;
            DatabaseReady = false;
            Waiting = 0;
        }

        public async Task AwaitDatabaseReadyAsync(CancellationToken cancellationToken)
        {
            while (DatabaseReady == false)
            {
                _logger.LogDebug($"Database not ready, waiting...");
                lock (Lock)
                {
                    Waiting++;
                }
                await _databaseReadySignal.WaitAsync(cancellationToken);
                _logger.LogDebug($"Got database ready signal");
            }
        }

        public void SetDatabaseBusy()
        {
            lock (Lock)
            {
                DatabaseReady = false;
                _logger.LogDebug($"Database set to busy");
            }
        }

        public void SetDatabaseReady()
        {
            lock (Lock)
            {
                DatabaseReady = true;
                _logger.LogDebug($"Database set to ready");
                _databaseReadySignal.Release(Waiting);
            }
        }

        public async Task AwaitDatabaseOnlineAsync(SqlConnection connection)
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
                    _logger.LogDebug($"Database {connection.Database} not online, waiting...");
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
    }
}
