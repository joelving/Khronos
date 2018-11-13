using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Khronos.Web.Server.Services
{
    public interface IBackgroundJobProcessor<T>
    {
        Task ProcessJob((T job, Action callback) job, CancellationToken cancellationToken);
    }
    
}
