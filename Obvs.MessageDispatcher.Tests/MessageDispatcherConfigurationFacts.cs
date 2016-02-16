using System;
using System.Reactive.Subjects;
using FluentAssertions;
using Moq;
using Obvs.MessageDispatcher.Configuration;
using Obvs.Types;
using Xunit;

namespace Obvs.MessageDispatcher.Tests
{
    public class MessageDispatcherConfigurationFacts
    {
        public class ConstructorFacts
        {
            [Fact]
            public void ThrowsOnNullMessagesObservable()
            {
                Action action = () => new MessageDispatcherConfiguration<IMessage>(null);

                action.ShouldThrow<ArgumentNullException>()
                    .And.ParamName.Should().Be("messages");
            }
        }

        public class WithMessageHandlerSelectorFactoryFacts
        {
            [Fact]
            public void ThrowsOnNullFactory()
            {
                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(Mock.Of<IObservable<ICommand>>());

                var messageDispatcherConfiguration = serviceBusMock.Object.WithDispatcherFor(sb => sb.Commands);

                Action action = () => messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(null);

                action.ShouldThrow<ArgumentNullException>()
                    .And.ParamName.Should().Be("messageHandlerSelectorFactory");
            }

            [Fact]
            public void ReturnsMessageDispatcherConfiguration()
            {
                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(Mock.Of<IObservable<ICommand>>());

                var messageDispatcherConfiguration = serviceBusMock.Object.WithDispatcherFor(sb => sb.Commands);

                var updatedConfiguration = messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(() => Mock.Of<IMessageHandlerSelector>());

                updatedConfiguration.Should().NotBeNull();
            }

            [Fact]
            public void MessageDispatcherConfigurationMessageHandlerShouldBeTheConfiguredFactory()
            {
                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(Mock.Of<IObservable<ICommand>>());

                var messageDispatcherConfiguration = serviceBusMock.Object.WithDispatcherFor(sb => sb.Commands);

                Func<IMessageHandlerSelector> messageHandlerSelectorFactory = () => Mock.Of<IMessageHandlerSelector>();

                var updatedConfiguration = messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(messageHandlerSelectorFactory);

                ((MessageDispatcherConfiguration<ICommand>)updatedConfiguration).MessageHandlerSelectorFactory.Should().Be(messageHandlerSelectorFactory);
            }
        }

        public class DispatchMessagesWithNoMessageDispatchResultActionFacts
        {
            [Fact]
            public void SubscribesToUnderlyingObservable()
            {
                var commandsObservableMock = new Mock<IObservable<ICommand>>();

                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(commandsObservableMock.Object);

                var messageDispatcherConfiguration = serviceBusMock.Object.WithDispatcherFor(sb => sb.Commands);

                messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(() => Mock.Of<IMessageHandlerSelector>())
                    .DispatchMessages();

                commandsObservableMock.Verify(co => co.Subscribe(It.IsNotNull<IObserver<ICommand>>()), Times.Once());
            }
        }

        public class DispatchMessagesWithMessageDispatchResultActionFacts
        {
            [Fact]
            public void SubscribesToUnderlyingObservable()
            {
                var commandsObservableMock = new Mock<IObservable<ICommand>>();

                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(commandsObservableMock.Object);

                var messageDispatcherConfiguration = serviceBusMock.Object.WithDispatcherFor(sb => sb.Commands);

                messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(() => Mock.Of<IMessageHandlerSelector>())
                    .DispatchMessages(Mock.Of<Action<MessageDispatchResult<ICommand>>>());

                commandsObservableMock.Verify(co => co.Subscribe(It.IsNotNull<IObserver<ICommand>>()), Times.Once());
            }

            [Fact]
            public void PropagatesMessageDispatchResultToSuppliedAction()
            {
                var commandsSubject = new Subject<ICommand>();

                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(commandsSubject);

                var messageDispatcherConfiguration = serviceBusMock.Object.WithDispatcherFor(sb => sb.Commands);

                var messageDispatchResultActionMock = new Mock<Action<MessageDispatchResult<ICommand>>>();

                messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(() => Mock.Of<IMessageHandlerSelector>())
                    .DispatchMessages(messageDispatchResultActionMock.Object);

                var command = Mock.Of<ICommand>();

                commandsSubject.OnNext(command);

                messageDispatchResultActionMock.Verify(a => a(It.Is<MessageDispatchResult<ICommand>>(mdr => Object.ReferenceEquals(mdr.Message, command))), Times.Once());
            }
        }
    }

    public class DispatcherForFacts
    {
        [Fact]
        public void ThrowsOnNullServiceBus()
        {
            IServiceBus serviceBus = null;

            Action action = () => serviceBus.WithDispatcherFor(sb => sb.Commands);

            action.ShouldThrow<ArgumentNullException>()
                .And.ParamName.Should().Be("serviceBus");
        }

        [Fact]
        public void ReturnsMessageDispatcherConfiguration()
        {
            var serviceBusMock = new Mock<IServiceBus>();
            serviceBusMock.SetupGet(sb => sb.Commands)
                .Returns(Mock.Of<IObservable<ICommand>>());

            var messageDispatcherConfiguration = serviceBusMock.Object.WithDispatcherFor(sb => sb.Commands);

            messageDispatcherConfiguration.Should().NotBeNull();
        }

        [Fact]
        public void MessageDispatcherConfigurationMessagesShouldBeTheSelectedMessages()
        {
            var commandsObservableMock = Mock.Of<IObservable<ICommand>>();

            var serviceBusMock = new Mock<IServiceBus>();
            serviceBusMock.SetupGet(sb => sb.Commands)
                .Returns(commandsObservableMock);

            var messageDispatcherConfiguration = serviceBusMock.Object.WithDispatcherFor(sb => sb.Commands);

            ((MessageDispatcherConfiguration<ICommand>)messageDispatcherConfiguration).Messages.Should().Be(commandsObservableMock);
        }
    }
}
