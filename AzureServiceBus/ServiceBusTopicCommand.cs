using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServiceBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceBus
{
    public class ServiceBusTopicCommand<T> : IServiceBusTopicCommand<T> where T : class
    {

        private readonly ILogger logger;

        private string connectionString;
        private string topicName;
        private string subscriptionName;

        public ServiceBusTopicCommand(ILoggerFactory loggerFactory, string connectionString, string topicName)
        {
            if (loggerFactory is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or empty.", nameof(connectionString));
            }

            if (string.IsNullOrEmpty(topicName))
            {
                throw new ArgumentException($"'{nameof(topicName)}' cannot be null or empty.", nameof(topicName));
            }

            this.logger = loggerFactory.CreateLogger<ServiceBusTopicHandler<T>>();
            this.connectionString = connectionString;
            this.topicName = topicName;
        }

        public async Task<Exception> Send(T messageObject)
        {
            try
            {
                await using var client = new ServiceBusClient(connectionString);
                await using var sender = client.CreateSender(topicName);

                // Create a new message to send to the topic
                var messageBody = JsonConvert.SerializeObject(messageObject);
                var message = new ServiceBusMessage(messageBody);

                // Write the body of the message to the console
                Console.WriteLine($"Sending message: {messageBody}");

                // Send the message to the topic
                await sender.SendMessageAsync(message);

                // Close the sender
                await sender.CloseAsync();
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError($"Service bus message command {this.topicName} encountered an exception: {ex}.");
                return ex;
            }
        }
    }
}
