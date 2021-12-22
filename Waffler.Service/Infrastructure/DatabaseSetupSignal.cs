using System.Threading;
using System.Threading.Tasks;

namespace Waffler.Service.Infrastructure
{
    public class DatabaseSetupSignal
    {
        private readonly SemaphoreSlim _databaseReadySignal;

        private bool DatabaseReady;
        private short Waiting;

        public DatabaseSetupSignal()
        {
            _databaseReadySignal = new SemaphoreSlim(0);
            DatabaseReady = false;
            Waiting = 0;
        }

        public async Task AwaitDatabaseReadyAsync(CancellationToken cancellationToken)
        {
            while(DatabaseReady == false)
            {
                Waiting++;
                await _databaseReadySignal.WaitAsync(cancellationToken);
            }
        }

        public void SetDatabaseReady()
        {
            DatabaseReady = true;
            _databaseReadySignal.Release(Waiting);
        }
    }
}
