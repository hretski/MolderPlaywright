using System;
using System.Threading.Tasks;

namespace Molder.Web.Models.Mediator
{
    public interface IMediator
    {
        Task ExecuteAsync(Action action);
        Task<object> ExecuteAsync<TResult>(Func<TResult> action);
        Task<object> WaitAsync<TResult>(Func<TResult> action);
    }
}
