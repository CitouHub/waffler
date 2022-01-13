using System.Threading;
using System.Threading.Tasks;

namespace Waffler.Service.Infrastructure
{
    public interface ICandleStickSyncSignal
    {
        void StartSync();
        void Abort();
        bool IsActive();
        bool IsAbortRequested();
        Task AwaitAbortAsync();
        void CloseSync();
        void Throttle(bool throttled);
        bool IsThrottled();
    }

    public class CandleStickSyncSignal : ICandleStickSyncSignal
    {
        private readonly object SyncLock = new object();
        private bool Active = false;
        private bool Throttled = false;
        private bool AbortCurrentSync = false;
        private SemaphoreSlim SyncSignal = new SemaphoreSlim(0);

        public void StartSync()
        {
            lock(SyncLock)
            {
                Active = true;
                Throttled = false;
                AbortCurrentSync = false;
                SyncSignal = new SemaphoreSlim(0);
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

        public async Task AwaitAbortAsync()
        {
            await SyncSignal.WaitAsync();
        }

        public void CloseSync()
        {
            lock (SyncLock)
            {
                Active = false;
                Throttled = false;
                AbortCurrentSync = false;
                SyncSignal.Release();
            }
        }
    }
}
