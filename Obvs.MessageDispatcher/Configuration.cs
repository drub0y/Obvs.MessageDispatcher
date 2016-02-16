using System;
using Obvs.MessageDispatcher;

namespace Obvs.MessageDispatcher.Configuration
{
    public interface IMessageDispatcherConfigurationWithFactory<TMessage> : IMessageDispatcherConfiguration<TMessage>
        where TMessage : class
    {
        void DispatchMessages();
        void DispatchMessages(Action<MessageDispatchResult<TMessage>> onNextMessageDispatchResult);
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

        public void DispatchMessages()
        {
            _messages.DispatchMessages(_messageHandlerSelectorFactory);
        }

        public void DispatchMessages(Action<MessageDispatchResult<TMessage>> onNextMessageDispatchResult)
        {
            _messages.DispatchMessages(_messageHandlerSelectorFactory, onNextMessageDispatchResult);
        }
    }
}
