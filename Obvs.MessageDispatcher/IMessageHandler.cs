using Obvs.Types;
using System.Threading;
using System.Threading.Tasks;

namespace Obvs.MessageDispatcher
{
	public interface IMessageHandler
	{

	}

	public interface IMessageHandler<TMessage> : IMessageHandler
		where TMessage : IMessage
	{
		Task HandleAsync(TMessage message, CancellationToken cancellationToken);
	}
}
