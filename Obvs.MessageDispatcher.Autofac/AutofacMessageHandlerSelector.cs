using System;
using Autofac;

namespace Obvs.MessageDispatcher.Autofac
{
    internal sealed class AutofacMessageHandlerSelector : IMessageHandlerSelector, IDisposable
    {
        private readonly ILifetimeScope _lifetimescope;

        public AutofacMessageHandlerSelector(ILifetimeScope lifetimeScope)
        {
            _lifetimescope = lifetimeScope;
        }

        public void Dispose()
        {
            _lifetimescope.Dispose();
        }

        public IMessageHandler<TMessage> SelectMessageHandler<TMessage>(TMessage message)
        {
            return _lifetimescope.Resolve<IMessageHandler<TMessage>>();
        }
    }
}