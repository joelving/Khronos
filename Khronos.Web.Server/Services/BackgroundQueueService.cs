using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Khronos.Web.Server.Services
{
    public class BackgroundQueueService<T> : BackgroundService
    {
        public IBackgroundQueue<T> TaskQueue { get; }
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public BackgroundQueueService(IBackgroundQueue<T> taskQueue, ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory)
        {
            TaskQueue = taskQueue;
            _logger = loggerFactory.CreateLogger<BackgroundQueueService<T>>();
            _scopeFactory = scopeFactory;
        }

        private string ThreadKind
            => Thread.CurrentThread.IsThreadPoolThread ? "thread pool" : "non-thread pool";
        private string ThreadDescription
            => $"thread {Thread.CurrentThread.ManagedThreadId} which is a {ThreadKind} thread";
        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Queue Service is starting on {ThreadDescription}.");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                var item = await TaskQueue.DequeueAsync(cancellationToken);

                Task.Run(async () =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IBackgroundJobProcessor<T>>>();
                        logger.LogInformation($"Processing job on thread {Thread.CurrentThread.ManagedThreadId} which is a {ThreadKind} thread.");
                        var processor = scope.ServiceProvider.GetRequiredService<IBackgroundJobProcessor<T>>();

                        // The queue is running on it's own thread, dispatching jobs to the thread pool. This is fine since the processing is async and non-blocking.
                        await processor.ProcessJob(item, cancellationToken);
                    }
                }, cancellationToken);
            }

            _logger.LogInformation("Queue Service is stopping.");
        }
    }
}
