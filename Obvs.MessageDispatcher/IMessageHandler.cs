using System.Threading;
using System.Threading.Tasks;

namespace Obvs.MessageDispatcher
{
    public interface IMessageHandler
	{

	}

	public interface IMessageHandler<TMessage> : IMessageHandler
	{
		Task HandleAsync(TMessage message, CancellationToken cancellationToken);
	}
}
