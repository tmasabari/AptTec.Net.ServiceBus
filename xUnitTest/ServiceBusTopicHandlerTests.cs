using AutoFixture;
using AzureServiceBus;
using System.Reflection.Metadata;
using xUnitTest.TestCode;

namespace xUnitTest
{
    public class ServiceBusTopicHandlerTests
    {
        private readonly TestConfiguration _testConfig;

        public ServiceBusTopicHandlerTests()
        {
            _testConfig = new TestConfiguration();
        }
        [Fact]
        public async Task HandleMessageTest()
        {

            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();


            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var messageCommand = new ServiceBusTopicCommand<TestMessageContract>(loggerFactory,
                _testConfig.Configs["BusConnectionString"],
                "unit-test-topic");
            var messageHandler = new ServiceBusTopicHandler<TestMessageContract>(loggerFactory);
            // https://github.com/AutoFixture/AutoFixture/wiki/Cheat-Sheet
            var fixture = new Fixture();
            var autoObject = fixture.Create<TestMessageContract>();

            // Act
            var exception = await messageCommand.Send(autoObject);

            // Assert
            Assert.True(exception == null);

            await messageHandler.SubscribeTopic(_testConfig.Configs["BusConnectionString"],
                "unit-test-topic", "unit-test-topic-subscription", TestMessageHandlers.TestMessageContractProcessor);
        }
    }
}