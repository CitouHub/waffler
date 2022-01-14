using System.Threading;
using System.Threading.Tasks;

namespace Waffler.Service.Infrastructure
{
    public interface ICandleStickSyncSignal
    {
        void StartSync();
        void Abort();
        void Throttle(bool throttled);
        bool IsActive();
        bool IsAbortRequested();
        bool IsThrottled();
        void SyncComplete();
        void CloseSync();        
        Task AwaitAbortAsync(CancellationToken cancellationToken);
        Task AwaitSyncCompleteAsync(CancellationToken cancellationToken);
    }

    public class CandleStickSyncSignal : ICandleStickSyncSignal
    {
        private readonly SemaphoreSlim AbortSignal = new SemaphoreSlim(0);
        private readonly SemaphoreSlim SyncSignal = new SemaphoreSlim(0);
        private readonly object SyncLock = new object();

        private bool Active = false;
        private bool Throttled = false;
        private bool AbortCurrentSync = false;

        public void StartSync()
        {
            lock(SyncLock)
            {
                Active = true;
                Throttled = false;
                AbortCurrentSync = false;
            }
        }

        public void Abort()
        {
            AbortCurrentSync = true;
        }

        public bool IsActive()
        {
            return Active;
        }

        public void Throttle(bool throttle)
        {
            Throttled = throttle;
        }

        public bool IsThrottled()
        {
            return Throttled;
        }

        public bool IsAbortRequested()
        {
            return AbortCurrentSync;
        }

        public void SyncComplete()
        {
            SyncSignal.Release();
        }

        public void CloseSync()
        {
            lock (SyncLock)
            {
                Active = false;
                Throttled = false;
                AbortCurrentSync = false;
                AbortSignal.Release();
            }
        }

        public async Task AwaitAbortAsync(CancellationToken cancellationToken)
        {
            await AbortSignal.WaitAsync(cancellationToken);
        }

        public async Task AwaitSyncCompleteAsync(CancellationToken cancellationToken)
        {
            await SyncSignal.WaitAsync(cancellationToken);
        }
    }
}
