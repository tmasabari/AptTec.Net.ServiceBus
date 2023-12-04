// See https://aka.ms/new-console-template for more information
using AutoFixture;
using AzureServiceBus;
using xUnitTest.TestCode;

Console.WriteLine("Starting the console tester");
var serviceProvider = new ServiceCollection()
    .AddLogging()
    .BuildServiceProvider();

var _testConfig = new TestConfiguration();
var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

var messageHandler = new ServiceBusTopicHandler<TestMessageContract>(loggerFactory); 

await messageHandler.SubscribeTopic(_testConfig.Configs["BusConnectionString"],
    "unit-test-topic", "unit-test-topic-subscription", TestMessageHandlers.TestMessageContractProcessor);

// Wait for a key press
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
