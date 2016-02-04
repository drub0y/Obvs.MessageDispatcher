using System;

namespace Obvs.MessageDispatcher.Configuration
{
    public interface IMessageDispatcherConfigurationWithFactory<TMessage> : IMessageDispatcherConfiguration<TMessage>
        where TMessage : class
    {
        IObservable<MessageDispatchResult<TMessage>> RunDispatcher();
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

        public IObservable<MessageDispatchResult<TMessage>> RunDispatcher()
        {
            return new MessageDispatcher<TMessage>(_messageHandlerSelectorFactory).Run(_messages);
        }
    }


    public static class ServiceBusExtensions
    {
        public static IMessageDispatcherConfiguration<TTargetMessage> DispatcherFor<TMessage, TCommand, TEvent, TRequest, TResponse, TTargetMessage>(this IServiceBus<TMessage, TCommand, TEvent, TRequest, TResponse> serviceBus, Func<IServiceBus<TMessage, TCommand, TEvent, TRequest, TResponse>, IObservable<TTargetMessage>> messageSelector)
            where TMessage : class
            where TCommand : class, TMessage
            where TEvent : class, TMessage
            where TRequest : class, TMessage
            where TResponse : class, TMessage
            where TTargetMessage : class, TMessage
        {
            if(serviceBus == null) throw new ArgumentNullException(nameof(serviceBus));

            return new MessageDispatcherConfiguration<TTargetMessage>(messageSelector(serviceBus));
        }

        public static IMessageDispatcherConfiguration<TTargetMessage> DispatcherFor<TMessage, TCommand, TEvent, TRequest, TResponse, TTargetMessage>(this IServiceBusClient<TMessage, TCommand, TEvent, TRequest, TResponse> serviceBusClient, Func<IServiceBusClient<TMessage, TCommand, TEvent, TRequest, TResponse>, IObservable<TTargetMessage>> messageSelector)
            where TMessage : class
            where TCommand : class, TMessage
            where TEvent : class, TMessage
            where TRequest : class, TMessage
            where TResponse : class, TMessage
            where TTargetMessage : class, TMessage
        {
            if(serviceBusClient == null) throw new ArgumentNullException(nameof(serviceBusClient));

            return new MessageDispatcherConfiguration<TTargetMessage>(messageSelector(serviceBusClient));
        }
    }

    public static class ServiceEndpointExtensions
    {
        public static IMessageDispatcherConfiguration<TCommand> DispatcherForCommands<TMessage, TCommand, TEvent, TRequest, TResponse>(this IServiceEndpoint<TMessage, TCommand, TEvent, TRequest, TResponse> serviceEndpoint)
            where TMessage : class
            where TCommand : class, TMessage
            where TEvent : class, TMessage
            where TRequest : class, TMessage
            where TResponse : class, TMessage
        {
            if(serviceEndpoint == null) throw new ArgumentNullException(nameof(serviceEndpoint));

            return new MessageDispatcherConfiguration<TCommand>(serviceEndpoint.Commands);
        }

        public static IMessageDispatcherConfiguration<TEvent> DispatcherForEvents<TMessage, TCommand, TEvent, TRequest, TResponse>(this IServiceEndpointClient<TMessage, TCommand, TEvent, TRequest, TResponse> serviceEndpointClient)
            where TMessage : class
            where TCommand : class, TMessage
            where TEvent : class, TMessage
            where TRequest : class, TMessage
            where TResponse : class, TMessage
        {
            if(serviceEndpointClient == null) throw new ArgumentNullException(nameof(serviceEndpointClient));

            return new MessageDispatcherConfiguration<TEvent>(serviceEndpointClient.Events);
        }

    }
}
