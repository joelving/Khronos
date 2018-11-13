using Khronos.Shared;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Khronos.Web.Server.Services
{
    public class SyncJobQueue : IBackgroundQueue<SyncJob>
    {
        private ConcurrentQueue<SyncJob> _workItems = new ConcurrentQueue<SyncJob>();
        private SemaphoreSlim _queuedItems = new SemaphoreSlim(0);
        private SemaphoreSlim _maxQueueSize;

        public SyncJobQueue(int maxQueueSize)
        {
            _maxQueueSize = new SemaphoreSlim(maxQueueSize);
        }

        public async Task EnqueueAsync(SyncJob job, CancellationToken cancellationToken)
        {
            if (job == null)
                throw new ArgumentNullException(nameof(job));

            // This causes callers to wait until there's room in the queue.
            await _maxQueueSize.WaitAsync(cancellationToken);
            _workItems.Enqueue(job);
            _queuedItems.Release();
        }

        public async Task<(SyncJob job, Action callback)> DequeueAsync(CancellationToken cancellationToken)
        {
            // This ensures we can never dequeue unless the semaphore has been increased by a corresponding release.
            await _queuedItems.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var job);

            return (job, () => _maxQueueSize.Release());
        }
    }
}
