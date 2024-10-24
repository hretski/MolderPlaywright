using Polly.Retry;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Molder.Web.Models.Mediator
{
    [ExcludeFromCodeCoverage]
    public abstract class Mediator : IMediator
    {
        protected RetryPolicy retryPolicy = null;

        protected RetryPolicy waitAndRetryPolicy = null;

        public virtual async Task ExecuteAsync(Action action)
        {
            retryPolicy.Execute(action);
            await Task.CompletedTask;
        }

        public virtual async Task<object> ExecuteAsync<TResult>(Func<TResult> action)
        {
            return await Task.FromResult(retryPolicy.Execute(action));
        }

        public virtual async Task<object> WaitAsync<TResult>(Func<TResult> action)
        {
            return await Task.FromResult(waitAndRetryPolicy.Execute(action));
        }
    }
}