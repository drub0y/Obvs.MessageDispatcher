using System;
using System.Linq;
using System.Reflection;

namespace Obvs.MessageDispatcher.Configuration
{
    internal class SimpleMessageHandlerSelectorFactoryConfiguration<TMessage> : ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage>
        where TMessage : class
    {
        private readonly SimpleMessageHandlerSelector _simpleMessageHandlerSelector;

        public SimpleMessageHandlerSelectorFactoryConfiguration(SimpleMessageHandlerSelector simpleMessageHandlerSelector)
        {
            _simpleMessageHandlerSelector = simpleMessageHandlerSelector;
        }

        public ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler<TMessageHandler>() where TMessageHandler : IMessageHandler
        {
            RegisterMessageHandler(typeof(TMessageHandler));

            return this;
        }

        public ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler(Type messageHandler)
        {
            _simpleMessageHandlerSelector.RegisterMessageHandler(messageHandler);

            return this;
        }

        public ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandlers(params Assembly[] assembliesToScan)
        {
            if(assembliesToScan == null) throw new ArgumentNullException(nameof(assembliesToScan));

            var exportedMessageHandlerTypes = from assembly in assembliesToScan
                                                from type in assembly.GetExportedTypes()
                                                where typeof(IMessageHandler).IsAssignableFrom(type)
                                                                &&
                                                            type.GetConstructor(Type.EmptyTypes) != null
                                                select type;

            foreach(var messageHandlerType in exportedMessageHandlerTypes)
            {
                _simpleMessageHandlerSelector.RegisterMessageHandler(messageHandlerType);
            }

            return this;
        }

        public ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandlers(params Type[] messageHandlerTypes)
        {
            if(messageHandlerTypes == null) throw new ArgumentNullException(nameof(messageHandlerTypes));

            foreach(var messageHandlerType in messageHandlerTypes)
            {
                _simpleMessageHandlerSelector.RegisterMessageHandler(messageHandlerType);
            }

            return this;
        }

        public ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler(IMessageHandler messageHandler)
        {
            _simpleMessageHandlerSelector.RegisterMessageHandler(messageHandler);

            return this;
        }

        public ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler<TMessageHandler>(Func<TMessageHandler> messageHandler) 
            where TMessageHandler : class, IMessageHandler<TMessage>
        {
            _simpleMessageHandlerSelector.RegisterMessageHandler<TMessageHandler>(messageHandler);

            return this;
        }
    }

    public interface ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage>
        where TMessage : class
    {
        ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler<TMessageHandler>() where TMessageHandler : IMessageHandler;

        ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler(Type messageHandlerType);

        ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandlers(params Type[] messageHandlerTypes);

        ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandlers(params Assembly[] assembly);

        ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler(IMessageHandler messageHandler);

        ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage> RegisterMessageHandler<TMessageHandler>(Func<TMessageHandler> messageHandler) where TMessageHandler : class, IMessageHandler<TMessage>;
    }

    public static class SimpleMessageHandlerSelectorMessageDispatcherConfigurationExtensions
    {
        public static ISimpleMessageHandlerSelectorFactoryConfiguration<TMessage> WithSimpleMessageHandlerSelectorFactory<TMessage>(this IMessageDispatcherConfiguration<TMessage> messageDispatcherConfiguration)
            where TMessage : class
        {
            var simpleMessageHandlerSelector = new SimpleMessageHandlerSelector();

            messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(() => simpleMessageHandlerSelector);

            return new SimpleMessageHandlerSelectorFactoryConfiguration<TMessage>(simpleMessageHandlerSelector);
        }
    }
}
