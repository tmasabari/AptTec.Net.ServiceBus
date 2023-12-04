using System;
using System.Threading.Tasks;

namespace ServiceBus.Abstractions
{
    public interface IServiceBusTopicCommand<T> where T : class
    {
        Task<Exception> Send(T messageObject);
    }
}