using Autofac;
using Obvs.MessageDispatcher.Configuration;

namespace Obvs.MessageDispatcher.Autofac
{
    public static class MessageDispatcherConfigurationExtensions
    {
        public static IMessageDispatcherConfigurationWithFactory<TMessage> WithAutofacMessageHandlerSelectorFactory<TMessage>(this IMessageDispatcherConfiguration<TMessage> messageDispatcherConfiguration, IContainer container) where TMessage : class
        {
            return messageDispatcherConfiguration.WithMessageHandlerSelectorFactory(() => new AutofacMessageHandlerSelector(container.BeginLifetimeScope()));
        }
    }
}
