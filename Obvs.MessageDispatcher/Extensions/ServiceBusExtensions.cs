using System;
using Obvs.MessageDispatcher.Configuration;

namespace Obvs.MessageDispatcher
{
    public static class ServiceBusExtensions
    {
        public static IMessageDispatcherConfiguration<TTargetMessage> WithDispatcherFor<TMessage, TCommand, TEvent, TRequest, TResponse, TTargetMessage>(this IServiceBus<TMessage, TCommand, TEvent, TRequest, TResponse> serviceBus, Func<IServiceBus<TMessage, TCommand, TEvent, TRequest, TResponse>, IObservable<TTargetMessage>> messageSelector)
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

        public static IMessageDispatcherConfiguration<TTargetMessage> WithDispatcherFor<TMessage, TCommand, TEvent, TRequest, TResponse, TTargetMessage>(this IServiceBusClient<TMessage, TCommand, TEvent, TRequest, TResponse> serviceBusClient, Func<IServiceBusClient<TMessage, TCommand, TEvent, TRequest, TResponse>, IObservable<TTargetMessage>> messageSelector)
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

}
