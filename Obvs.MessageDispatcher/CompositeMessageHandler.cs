using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Obvs.MessageDispatcher
{
    public sealed class CompositeMessageHandler<TMessage> : IMessageHandler<TMessage> where TMessage : class
    {
        private readonly IEnumerable<IMessageHandler<TMessage>> _handlers;
        private readonly Func<TMessage, CancellationToken, Task> _selectedHandleAsyncStrategy;

        public CompositeMessageHandler(IEnumerable<IMessageHandler<TMessage>> handlers, bool executeHandlersConcurrently)
        {
            _handlers = handlers;
            _selectedHandleAsyncStrategy = executeHandlersConcurrently ? (Func<TMessage, CancellationToken, Task>)HandleAsyncConcurrently : HandleAsyncSequentially;
        }

        public Task HandleAsync(TMessage message, CancellationToken cancellationToken)
        {
            return _selectedHandleAsyncStrategy(message, cancellationToken);
        }

        private async Task HandleAsyncConcurrently(TMessage message, CancellationToken cancellationToken)
        {
            await Task.WhenAll(_handlers.Select(h => h.HandleAsync(message, cancellationToken)));
        }

        private async Task HandleAsyncSequentially(TMessage message, CancellationToken cancellationToken)
        {
            foreach(IMessageHandler<TMessage> handler in _handlers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await handler.HandleAsync(message, cancellationToken);
            }
        }
    }
}
