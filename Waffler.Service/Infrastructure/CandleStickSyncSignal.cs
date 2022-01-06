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
    }

    public class CandleStickSyncSignal : ICandleStickSyncSignal
    {
        private readonly object SyncLock = new object();
        private bool Active = false;
        private bool AbortCurrentSync = false;
        private SemaphoreSlim SyncSignal = new SemaphoreSlim(0);

        public void StartSync()
        {
            lock(SyncLock)
            {
                Active = true;
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
                AbortCurrentSync = false;
                SyncSignal.Release();
            }
        }
    }
}
