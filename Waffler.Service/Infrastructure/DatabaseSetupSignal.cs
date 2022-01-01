using System.Threading;
using System.Threading.Tasks;

namespace Waffler.Service.Infrastructure
{
    public interface IDatabaseSetupSignal
    {
        Task AwaitDatabaseReadyAsync(CancellationToken cancellationToken);
        void SetDatabaseReady();
    }

    public class DatabaseSetupSignal : IDatabaseSetupSignal
    {
        private readonly SemaphoreSlim _databaseReadySignal;
        private readonly object Lock = new object();

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
            while (DatabaseReady == false)
            {
                lock (Lock)
                {
                    Waiting++;
                }
                await _databaseReadySignal.WaitAsync(cancellationToken);
            }
        }

        public void SetDatabaseReady()
        {
            lock (Lock)
            {
                DatabaseReady = true;
                _databaseReadySignal.Release(Waiting);
            }
        }
    }
}
