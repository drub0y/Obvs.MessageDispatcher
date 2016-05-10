using System;

namespace Obvs.MessageDispatcher.Configuration
{
    public interface IMessageDispatcherConfigurationWithFactory<TMessage> : IMessageDispatcherConfiguration<TMessage>
        where TMessage : class
    {
        IObservable<MessageDispatchResult<TMessage>> DispatchMessages();
    }

    public interface IMessageDispatcherConfiguration<TMessage>
        where TMessage : class
    {
        IMessageDispatcherConfigurationWithFactory<TMessage> WithMessageHandlerSelectorFactory(Func<IMessageHandlerSelector> messageHandlerSelectorFactory);
    }

    internal class MessageDispatcherConfiguration<TMessage> : IMessageDispatcherConfigurationWithFactory<TMessage>
        where TMessage : class
    {
        private readonly IObservable<TMessage> _messages;
        private Func<IMessageHandlerSelector> _messageHandlerSelectorFactory;

        public MessageDispatcherConfiguration(IObservable<TMessage> messages)
        {
            if(messages == null) throw new ArgumentNullException(nameof(messages));

            _messages = messages;
        }

        public IObservable<TMessage> Messages => _messages;
        public Func<IMessageHandlerSelector> MessageHandlerSelectorFactory => _messageHandlerSelectorFactory;

        public IMessageDispatcherConfigurationWithFactory<TMessage> WithMessageHandlerSelectorFactory(Func<IMessageHandlerSelector> messageHandlerSelectorFactory)
        {
            if(messageHandlerSelectorFactory == null) throw new ArgumentNullException(nameof(messageHandlerSelectorFactory));

            _messageHandlerSelectorFactory = messageHandlerSelectorFactory;

            return this;
        }

        public IObservable<MessageDispatchResult<TMessage>> DispatchMessages() => new MessageDispatcher<TMessage>(_messages, _messageHandlerSelectorFactory);
    }

    public static class MessageDispatcherConfigurationWithFactoryExtensions
    {
        public static IDisposable DispatchMessages<TMessage>(this IMessageDispatcherConfigurationWithFactory<TMessage> messageDispatcherConfigurationWithFactory, Action<MessageDispatchResult<TMessage>> onNextMessageDispatchResult, Action<Exception> onError, Action onCompleted) 
            where TMessage : class
        {
            return messageDispatcherConfigurationWithFactory.DispatchMessages().Subscribe(onNextMessageDispatchResult, onError, onCompleted);
        }

        public static IDisposable DispatchMessages<TMessage>(this IMessageDispatcherConfigurationWithFactory<TMessage> messageDispatcherConfigurationWithFactory, IObserver<MessageDispatchResult<TMessage>> observer)
            where TMessage : class
        {
            return messageDispatcherConfigurationWithFactory.DispatchMessages().Subscribe(observer);
        }
    }
}
