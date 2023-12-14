using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using ServiceBus.Abstractions;
using Microsoft.Azure.Amqp.Framing;
using System;

namespace AptTec.NET.AzureServiceBus
{
    /// <summary>
    /// No need to worry about creating the pump
    /// Auto-completion once the call back is successfully executed
    /// Auto-extension of lock duration if operation is taking longer
    /// Error handling-> move the message to dead letter queue in case of exception
    /// Easy control over concurrency
    /// https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions?tabs=connection-string
    /// The Service Bus client types are safe to cache and use as a singleton for the lifetime
    /// of the application, which is best practice when messages are being published or read regularly.
    /// </summary>
    public class ServiceBusTopicHandler<T> : IServiceBusTopicHandler<T> where T : class
    {
        private readonly ILogger logger;

        private Func<T, Task> handleMessageFunction;
        private string connectionString;
        private string topicName;
        private string subscriptionName;

        public ServiceBusTopicHandler(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<ServiceBusTopicHandler<T>>();

            if (loggerFactory is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="topicName"></param>
        /// <param name="subscriptionName"></param>
        /// <param name="handleMessageFunction"></param>
        /// <param name="MaxConcurrentCalls"></param>
        /// <param name="MaxAutoLockRenewalDuration">The dafault value is Timeout.InfiniteTimeSpan</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task SubscribeTopic(string connectionString, string topicName, string subscriptionName,
                Func<T, Task> handleMessageFunction, int MaxConcurrentCalls =1, TimeSpan MaxAutoLockRenewalDuration = default)
        {

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or empty.", nameof(connectionString));
            }

            if (string.IsNullOrEmpty(topicName))
            {
                throw new ArgumentException($"'{nameof(topicName)}' cannot be null or empty.", nameof(topicName));
            }

            if (string.IsNullOrEmpty(subscriptionName))
            {
                throw new ArgumentException($"'{nameof(subscriptionName)}' cannot be null or empty.", nameof(subscriptionName));
            }
            if (MaxAutoLockRenewalDuration == default)
            {
                MaxAutoLockRenewalDuration = Timeout.InfiniteTimeSpan; // TimeSpan.FromMinutes(10), //default value 5 minutes
            }
            this.connectionString = connectionString;
            this.topicName = topicName;
            this.subscriptionName = subscriptionName;
            this.handleMessageFunction = handleMessageFunction ?? throw new ArgumentNullException(nameof(handleMessageFunction));

            await Register(MaxConcurrentCalls, MaxAutoLockRenewalDuration);
        }


        private async Task Register(int MaxConcurrentCalls, TimeSpan MaxAutoLockRenewalDuration)
        {
            // Create a new Service Bus client
            var serviceBusClient = new ServiceBusClient(connectionString);
            var serviceBusProcessorOptions = new ServiceBusProcessorOptions()
            {
                Identifier = topicName,

                //The lock prevents other consumers from processing the same message simultaneously.
                //If the consumer fails to complete the message within the lock duration, the message is released back to the queue or subscription for another consumer to process.
                ReceiveMode = ServiceBusReceiveMode.PeekLock,

                //Lock renewal involves sending periodic requests to the Service Bus service to extend the lock duration.
                //the maximum duration for which the lock on a message can be automatically renewed.
                //Longer lock renewal durations might lead to increased contention for messages, as the lock on a message is held for an extended period, potentially affecting the ability of other consumers to process messages.
                //If one consumer holds a lock for an extended period, other consumers may be starved of the opportunity to process messages from the same subscription or queue.
                MaxAutoLockRenewalDuration = MaxAutoLockRenewalDuration, 

                // Maximum number of concurrent calls to the callback ProcessMessagesAsync (),
                // If you have three instances of your message processor, and each instance
                // is configured with MaxConcurrentCalls = 1, then each instance will fetch and process
                // one message at a time.  the three instances will collectively handle up to three messages concurrently.
                MaxConcurrentCalls = MaxConcurrentCalls,

                //  the message handler triggers an exception and did not settle the message,
                // then the message will be automatically abandoned, irrespective of AutoCompleteMessages
                AutoCompleteMessages = false
            };

            // Create a processor that we can use to process the messages
            ServiceBusProcessor processor = serviceBusClient.CreateProcessor(topicName, subscriptionName
                , serviceBusProcessorOptions);

            // Add handler to process messages
            processor.ProcessMessageAsync += ProcessMessagesAsync;

            // Add handler to process any errors
            processor.ProcessErrorAsync += ProcessErrorAsync;

            // Start processing 
            await processor.StartProcessingAsync();
        }
        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            try
            {
                // Process the message here
                logger.LogInformation($"Service bus message handler {this.topicName} received message: SequenceNumber:{message.SequenceNumber}"); 
                // Body:{message.Body}
                // Convert the message body to the specified generic type
                var messageBody = JsonConvert.DeserializeObject<T>(message.Body.ToString());

                // Call the provided generic function to handle the message. will be invoked concurrently by the SDK
                // if MaxConcurrentCalls is set to a value greater than 1.

                // Therefore, it's important to ensure that the handleMessageFunction is thread-safe if it involves shared resources, mutable state, or any operation that might lead to race conditions.

                // If the handleMessageFunction uses shared state or mutable data structures,
                // make sure that access to these resources is properly synchronized using
                // locks, mutexes, or other synchronization mechanisms to prevent data corruption.

                //If possible, design your handleMessageFunction to work with immutable data structures or data that is not modified during processing. Immutability can simplify concurrency concerns.

                await handleMessageFunction(messageBody);

                // Complete the message
                //After a message is marked as complete, it is removed from the queue or subscription and won't be delivered to any other consumers.
                await args.CompleteMessageAsync(message);
            }
            catch (Exception ex)
            {
                // Handle the exception
                logger.LogError($"Service bus message handler {this.topicName} : SequenceNumber:{message.SequenceNumber} : encountered an exception: {ex}");

                //When an exception occurs, consider abandoning the message or
                ////moving it to a Dead-Letter Queue (DLQ) after a certain number of failed attempts.
                await args.DeadLetterMessageAsync(message, $"Reason for dead lettering {ex}");
            }

        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            logger.LogError($"Service bus message handler {this.topicName} :  triggered ProcessErrorAsync: {args}.");
            return Task.CompletedTask;
        }
    }

}
