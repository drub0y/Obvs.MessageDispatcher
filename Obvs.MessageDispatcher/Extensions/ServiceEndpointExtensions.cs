using System;
using Obvs.MessageDispatcher.Configuration;

namespace Obvs.MessageDispatcher
{
    public static class ServiceEndpointExtensions
    {
        public static IMessageDispatcherConfiguration<TCommand> CreateCommandDispatcher<TMessage, TCommand, TEvent, TRequest, TResponse>(this IServiceEndpoint<TMessage, TCommand, TEvent, TRequest, TResponse> serviceEndpoint)
            where TMessage : class
            where TCommand : class, TMessage
            where TEvent : class, TMessage
            where TRequest : class, TMessage
            where TResponse : class, TMessage
        {
            if(serviceEndpoint == null) throw new ArgumentNullException(nameof(serviceEndpoint));

            return new MessageDispatcherConfiguration<TCommand>(serviceEndpoint.Commands);
        }

        public static IMessageDispatcherConfiguration<TEvent> CreateEventDispatcher<TMessage, TCommand, TEvent, TRequest, TResponse>(this IServiceEndpointClient<TMessage, TCommand, TEvent, TRequest, TResponse> serviceEndpointClient)
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
