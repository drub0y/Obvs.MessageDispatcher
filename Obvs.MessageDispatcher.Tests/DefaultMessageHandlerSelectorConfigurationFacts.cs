using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace Obvs.MessageDispatcher.Configuration.Tests
{
    public class DefaultMessageHandlerSelectorConfigurationFacts
    {
        private readonly Mock<IDefaultMessageHandlerSelector> _defaultMessageHandlerSelectorMock;
        private readonly DefaultMessageHandlerSelectorFactoryConfiguration<TestMessage> _defaultMessageHandlerSelectorFactoryConfiguration;

        public DefaultMessageHandlerSelectorConfigurationFacts()
        {
            _defaultMessageHandlerSelectorMock = new Mock<IDefaultMessageHandlerSelector>();
            _defaultMessageHandlerSelectorFactoryConfiguration = new DefaultMessageHandlerSelectorFactoryConfiguration<TestMessage>(_defaultMessageHandlerSelectorMock.Object);
        }

        public class ContructorFacts : DefaultMessageHandlerSelectorConfigurationFacts
        {
            [Fact]
            public void CreateWithValidDefaultMessageHandlerSelectorInstance()
            {
                new DefaultMessageHandlerSelectorFactoryConfiguration<TestMessage>(_defaultMessageHandlerSelectorMock.Object);
            }

            [Fact]
            public void ThrowsOnNullDefaultMessageHandler()
            {
                Action action = () => new DefaultMessageHandlerSelectorFactoryConfiguration<TestMessage>(null);

                action.ShouldThrow<ArgumentNullException>()
                    .And.ParamName.Should().Be("simpleMessageHandlerSelector");
            }
        }

        public class RegisterMessageHandlerGenericFacts : DefaultMessageHandlerSelectorConfigurationFacts
        {
            [Fact]
            public void RegisterMessageHandler()
            {
                _defaultMessageHandlerSelectorFactoryConfiguration.RegisterMessageHandler<TestMessageHandler>();

                _defaultMessageHandlerSelectorMock.Verify(smhs => smhs.RegisterMessageHandler(It.Is<Type>(it => it == typeof(TestMessageHandler))), Times.Once());
            }
        }

        public class RegisterMessageHandlerTypeFacts : DefaultMessageHandlerSelectorConfigurationFacts
        {
            [Fact]
            public void RegisterMessageHandlerType()
            {
                _defaultMessageHandlerSelectorFactoryConfiguration.RegisterMessageHandler(typeof(TestMessageHandler));

                _defaultMessageHandlerSelectorMock.Verify(smhs => smhs.RegisterMessageHandler(It.Is<Type>(it => it == typeof(TestMessageHandler))), Times.Once());
            }

            [Fact]
            public void RegisterMessageHandlerType_ThrowsOnNullMessageHandlerType()
            {
                Action action = () => _defaultMessageHandlerSelectorFactoryConfiguration.RegisterMessageHandler((Type)null);

                action.ShouldThrow<ArgumentNullException>()
                    .And.ParamName.Should().Be("messageHandlerType");
            }
        }

        public class RegisterMessageHandlerInstanceFacts : DefaultMessageHandlerSelectorConfigurationFacts
        {
            [Fact]
            public void RegisterMessageHandlerInstance()
            {
                var messageHandler = Mock.Of<IMessageHandler<TestMessage>>();

                _defaultMessageHandlerSelectorFactoryConfiguration.RegisterMessageHandler(messageHandler);

                _defaultMessageHandlerSelectorMock.Verify(smhs => smhs.RegisterMessageHandler(It.Is<IMessageHandler>(it => Object.ReferenceEquals(it, messageHandler))), Times.Once());
            }

            [Fact]
            public void RegisterMessageHandlerInstance_ThrowsOnNullMessageHandlerInstance()
            {
                Action action = () => _defaultMessageHandlerSelectorFactoryConfiguration.RegisterMessageHandler((IMessageHandler)null);

                action.ShouldThrow<ArgumentNullException>()
                    .And.ParamName.Should().Be("messageHandler");
            }
        }

        internal class TestMessage
        {

        }

        private class TestMessageHandler : IMessageHandler<TestMessage>
        {
            public Task HandleAsync(TestMessage message, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
