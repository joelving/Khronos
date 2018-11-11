using Khronos.Shared;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Khronos.Web.Server.Services
{
    public interface ISyncJobQueue
    {
        void QueueSyncJob(SyncJob job);

        Task<SyncJob> DequeueAsync(CancellationToken cancellationToken);
    }

    public class SyncJobQueue : ISyncJobQueue
    {
        private ConcurrentQueue<SyncJob> _workItems = new ConcurrentQueue<SyncJob>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueSyncJob(SyncJob job)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            _workItems.Enqueue(job);
            _signal.Release();
        }

        public async Task<SyncJob> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var job);

            return job;
        }
    }
}
