using System.Threading;
using System.Threading.Tasks;

namespace Waffler.Service.Infrastructure
{
    public class DatabaseSetupSignal
    {
        private readonly SemaphoreSlim _databaseReadySignal;

        private bool DatabaseReady;

        public DatabaseSetupSignal()
        {
            _databaseReadySignal = new SemaphoreSlim(0);
            DatabaseReady = false;
        }

        public async Task AwaitDatabaseReadyAsync(CancellationToken cancellationToken)
        {
            while(DatabaseReady == false)
            {
                await _databaseReadySignal.WaitAsync(cancellationToken);
            }
        }

        public void SetDatabaseReady()
        {
            DatabaseReady = true;
            _databaseReadySignal.Release();
        }
    }
}
