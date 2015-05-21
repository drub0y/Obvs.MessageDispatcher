using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reactive.Subjects;
using Obvs.Types;
using FluentAssertions;
using Xunit;
using System.Reactive.Disposables;
using System.Threading;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Obvs.MessageDispatcher.Tests
{
	public class MessageDispatcherFacts
	{
		public class RunFacts
		{
			[Fact]
			public void SubscribesToObservableMessageSource()
			{
				Mock<IMessageHandlerProvider> mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();

				MessageDispatcher dispatcher = new MessageDispatcher(mockMessageHandlerProvider.Object);

				Mock<IObservable<TestMessage>> testMessagesMock = new Mock<IObservable<TestMessage>>();
				testMessagesMock.Setup(it => it.Subscribe(It.IsAny<IObserver<TestMessage>>()))
					.Returns(Disposable.Empty);

				IObservable<MessageDispatchResult> messageDispatchResults = dispatcher.Run(testMessagesMock.Object);

				messageDispatchResults.Should().NotBeNull();

				messageDispatchResults.Subscribe();

				testMessagesMock.Verify(it => it.Subscribe(It.IsAny<IObserver<TestMessage>>()), Times.Once());
			}

			[Fact]
			public void DisposesSubscriptionOnObservableMessageSource()
			{
				Mock<IMessageHandlerProvider> mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();

				MessageDispatcher dispatcher = new MessageDispatcher(mockMessageHandlerProvider.Object);

				Mock<IDisposable> runAsyncDisposableMock = new Mock<IDisposable>();

				Mock<IObservable<TestMessage>> testMessagesMock = new Mock<IObservable<TestMessage>>();
				testMessagesMock.Setup(it => it.Subscribe(It.IsAny<IObserver<TestMessage>>()))
					.Returns(runAsyncDisposableMock.Object);

				IObservable<MessageDispatchResult> messageDispatchResults = dispatcher.Run(testMessagesMock.Object);

				messageDispatchResults.Subscribe().Dispose();

				runAsyncDisposableMock.Verify(it => it.Dispose(), Times.Once());
			}

			[Fact]
			public void InvokesMessageHandlerProvider()
			{
				Mock<IMessageHandlerProvider> mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();

				MessageDispatcher dispatcher = new MessageDispatcher(mockMessageHandlerProvider.Object);

				Subject<IMessage> messageSubject = new Subject<IMessage>();

				using (dispatcher.Run(messageSubject).Subscribe())
				{
					messageSubject.OnNext(new TestMessage());
				}
				
				mockMessageHandlerProvider.Verify(mhp => mhp.Provide<TestMessage>(), Times.Once());
			}

			[Fact]
			public void DoesntDispatchMessageWhichHasNoHandler()
			{
				Mock<IMessageHandlerProvider> mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();

				MessageDispatcher dispatcher = new MessageDispatcher(mockMessageHandlerProvider.Object);

				Subject<IMessage> messageSubject = new Subject<IMessage>();

				IObservable<MessageDispatchResult> messageDispatchResults = dispatcher.Run(messageSubject);

				TestMessage testMessage = new TestMessage();

				using (messageDispatchResults.Subscribe(mdr =>
				{
					mdr.Message.Should().BeSameAs(testMessage);
					mdr.Handled.Should().Be(false);
				}))
				{
					messageSubject.OnNext(testMessage);
				}
			}

			[Fact]
			public void InvokesHandleAsyncOnMessageHandler()
			{
				Mock<IMessageHandler<TestMessage>> mockMessageHandler = new Mock<IMessageHandler<TestMessage>>();

				Mock<IMessageHandlerProvider> mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();
				mockMessageHandlerProvider.Setup(mhp => mhp.Provide<TestMessage>())
					.Returns(mockMessageHandler.Object);

				MessageDispatcher dispatcher = new MessageDispatcher(mockMessageHandlerProvider.Object);

				Subject<IMessage> messageSubject = new Subject<IMessage>();

				TestMessage testMessage = new TestMessage();

				IObservable<MessageDispatchResult> messageDispatcherResults = dispatcher.Run(messageSubject);

				using(messageDispatcherResults.Subscribe(mdr =>
				{
					mdr.Message.Should().BeSameAs(testMessage);
					mdr.Handled.Should().Be(true);
				}))
				{
					messageSubject.OnNext(testMessage);
				}

				mockMessageHandler.Verify(mh => mh.HandleAsync(testMessage, It.IsAny<CancellationToken>()), Times.Once());
			}

			[Fact]
			public void DoesNotHandleMessagesBeforeOrAfterSubscription()
			{
				Mock<IMessageHandler<TestMessage>> mockMessageHandler = new Mock<IMessageHandler<TestMessage>>();

				Mock<IMessageHandlerProvider> mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();
				mockMessageHandlerProvider.Setup(mhp => mhp.Provide<TestMessage>())
					.Returns(mockMessageHandler.Object);

				MessageDispatcher dispatcher = new MessageDispatcher(mockMessageHandlerProvider.Object);

				Subject<IMessage> messageSubject = new Subject<IMessage>();

				TestMessage testMessage = new TestMessage();

				IObservable<MessageDispatchResult> messageDispatcherResults = dispatcher.Run(messageSubject);

				messageSubject.OnNext(testMessage);

				using (messageDispatcherResults.Subscribe())
				{
					messageSubject.OnNext(testMessage);
				}

				messageSubject.OnNext(testMessage);

				mockMessageHandler.Verify(mh => mh.HandleAsync(testMessage, It.IsAny<CancellationToken>()), Times.Once());
			}

			[Fact]
			public async Task ProcessesMessagesInCorrectOrder()
			{
				Mock<IMessageHandler<TestMessage>> mockMessageHandler = new Mock<IMessageHandler<TestMessage>>();

				Mock<IMessageHandlerProvider> mockMessageHandlerProvider = new Mock<IMessageHandlerProvider>();
				mockMessageHandlerProvider.Setup(mhp => mhp.Provide<TestMessage>())
					.Returns(mockMessageHandler.Object);

				MessageDispatcher dispatcher = new MessageDispatcher(mockMessageHandlerProvider.Object);

				TestMessage testMessage1 = new TestMessage();
				TestMessage testMessage2 = new TestMessage();

				IObservable<TestMessage> testMessages = (new[] { testMessage1, testMessage2 }).ToObservable();

				IObservable<MessageDispatchResult> messageDispatcherResults = dispatcher.Run(testMessages);

				bool areEqual = await messageDispatcherResults.Select(mdr => mdr.Message).SequenceEqual(testMessages).FirstAsync();

				areEqual.Should().Be(true);
			}
		}

		public class TestMessage : IMessage
		{

		}
	}
}
