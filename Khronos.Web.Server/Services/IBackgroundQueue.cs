using System;
using System.Threading;
using System.Threading.Tasks;

namespace Khronos.Web.Server.Services
{
    public interface IBackgroundQueue<T>
    {
        Task EnqueueAsync(T job, CancellationToken cancellationToken);

        Task<(T job, Action callback)> DequeueAsync(CancellationToken cancellationToken);
    }
}
