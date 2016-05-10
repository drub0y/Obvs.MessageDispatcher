using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
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

                var messageDispatcherConfiguration = serviceBusMock.Object.CreateDispatcherFor(sb => sb.Commands);

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

                var messageDispatcherConfiguration = serviceBusMock.Object.CreateDispatcherFor(sb => sb.Commands);

                var updatedConfiguration = messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(() => Mock.Of<IMessageHandlerSelector>());

                updatedConfiguration.Should().NotBeNull();
            }

            [Fact]
            public void MessageDispatcherConfigurationMessageHandlerShouldBeTheConfiguredFactory()
            {
                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(Mock.Of<IObservable<ICommand>>());

                var messageDispatcherConfiguration = serviceBusMock.Object.CreateDispatcherFor(sb => sb.Commands);

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

                var messageDispatcherConfiguration = serviceBusMock.Object.CreateDispatcherFor(sb => sb.Commands);

                messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(() => Mock.Of<IMessageHandlerSelector>())
                    .DispatchMessages()
                    .Subscribe()
                    .Dispose();

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

                var messageDispatcherConfiguration = serviceBusMock.Object.CreateDispatcherFor(sb => sb.Commands);

                messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(() => Mock.Of<IMessageHandlerSelector>())
                    .DispatchMessages(Mock.Of<Action<MessageDispatchResult<ICommand>>>(), Mock.Of<Action<Exception>>(), Mock.Of<Action>());

                commandsObservableMock.Verify(co => co.Subscribe(It.IsNotNull<IObserver<ICommand>>()), Times.Once());
            }

            [Fact]
            public void PropagatesMessageDispatchResultToSuppliedAction()
            {
                var commandsSubject = new Subject<ICommand>();

                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(commandsSubject);

                var messageDispatcherConfiguration = serviceBusMock.Object.CreateDispatcherFor(sb => sb.Commands);

                var messageDispatchResultActionMock = new Mock<Action<MessageDispatchResult<ICommand>>>();

                messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(() => Mock.Of<IMessageHandlerSelector>())
                    .DispatchMessages(messageDispatchResultActionMock.Object, Mock.Of<Action<Exception>>(), Mock.Of<Action>());

                var command = Mock.Of<ICommand>();

                commandsSubject.OnNext(command);

                messageDispatchResultActionMock.Verify(a => a(It.Is<MessageDispatchResult<ICommand>>(mdr => Object.ReferenceEquals(mdr.Message, command))), Times.Once());
            }
        }

        public class DispatchMessagesObservableFacts
        {
            [Fact]
            public void ReturnsNonNullObservable()
            {
                var commandsSubject = new Subject<ICommand>();

                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(commandsSubject);

                var messageDispatcherConfiguration = serviceBusMock.Object.CreateDispatcherFor(sb => sb.Commands)
                    .WithMessageHandlerSelectorFactory(() => Mock.Of<IMessageHandlerSelector>());

                var dispatchedMessages = messageDispatcherConfiguration.DispatchMessages();

                dispatchedMessages.Should().NotBeNull();
            }

            [Fact]
            public async Task PropagatesMessageDispatchResultToObserver()
            {
                var commandsSubject = new Subject<ICommand>();

                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(commandsSubject);

                var messageDispatcherConfiguration = serviceBusMock.Object.CreateDispatcherFor(sb => sb.Commands)
                    .WithMessageHandlerSelectorFactory(() => Mock.Of<IMessageHandlerSelector>());

                var dispatchedMessages = messageDispatcherConfiguration.DispatchMessages()
                                    .Replay();

                using(dispatchedMessages.Connect())
                {
                    var command = Mock.Of<ICommand>();

                    commandsSubject.OnNext(command);

                    var mdr = await dispatchedMessages.FirstOrDefaultAsync();

                    mdr.Should().NotBeNull();
                    mdr.Handled.Should().Be(false);
                    mdr.Message.Should().Be(command);
                }
            }

            [Fact]
            public void ObservableStreamPropagatesExceptionToObserverWhenMessageHandlerThrows()
            {
                var commandsSubject = new Subject<TestCommand>();

                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(commandsSubject);

                var messageHandlerException = new Exception();

                var messageHandlerMock = new Mock<IMessageHandler<TestCommand>>();
                messageHandlerMock.Setup(mh => mh.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
                    .Throws(messageHandlerException);

                var messageHandlerProviderMock = new Mock<IMessageHandlerSelector>();
                messageHandlerProviderMock.Setup(mhs => mhs.SelectMessageHandler<TestCommand>(It.IsAny<TestCommand>()))
                    .Returns(() => messageHandlerMock.Object);

                var messageDispatcherConfiguration = serviceBusMock.Object.CreateDispatcherFor(sb => sb.Commands)
                    .WithMessageHandlerSelectorFactory(() => messageHandlerProviderMock.Object);

                var onExceptionActionMock = new Mock<Action<Exception>>();

                using(messageDispatcherConfiguration.DispatchMessages()
                        .Subscribe(
                            Mock.Of<Action<MessageDispatchResult<ICommand>>>(),
                            onExceptionActionMock.Object,
                            Mock.Of<Action>()))
                {
                    commandsSubject.OnNext(new TestCommand());

                    onExceptionActionMock.Verify(oea => oea(It.Is<Exception>(e => Object.ReferenceEquals(e, messageHandlerException))), Times.Once());
                }
            }
        }

        public class DispatcherForFacts
        {
            [Fact]
            public void ThrowsOnNullServiceBus()
            {
                IServiceBus serviceBus = null;

                Action action = () => serviceBus.CreateDispatcherFor(sb => sb.Commands);

                action.ShouldThrow<ArgumentNullException>()
                    .And.ParamName.Should().Be("serviceBus");
            }

            [Fact]
            public void ReturnsMessageDispatcherConfiguration()
            {
                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(Mock.Of<IObservable<ICommand>>());

                var messageDispatcherConfiguration = serviceBusMock.Object.CreateDispatcherFor(sb => sb.Commands);

                messageDispatcherConfiguration.Should().NotBeNull();
            }

            [Fact]
            public void MessageDispatcherConfigurationMessagesShouldBeTheSelectedMessages()
            {
                var commandsObservableMock = Mock.Of<IObservable<ICommand>>();

                var serviceBusMock = new Mock<IServiceBus>();
                serviceBusMock.SetupGet(sb => sb.Commands)
                    .Returns(commandsObservableMock);

                var messageDispatcherConfiguration = serviceBusMock.Object.CreateDispatcherFor(sb => sb.Commands);

                ((MessageDispatcherConfiguration<ICommand>)messageDispatcherConfiguration).Messages.Should().Be(commandsObservableMock);
            }
        }

        public sealed class TestCommand : ICommand
        {

        }
    }
}
