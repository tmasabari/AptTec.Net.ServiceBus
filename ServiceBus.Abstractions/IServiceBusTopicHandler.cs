using System;
using System.Threading.Tasks;

namespace ServiceBus.Abstractions
{
    public interface IServiceBusTopicHandler<T> where T : class
    {
        Task SubscribeTopic(string connectionString, string topicName, string subscriptionName, Func<T, Task> handleMessageFunction, 
            int MaxConcurrentCalls = 1, TimeSpan MaxAutoLockRenewalDuration = default);
    }
}