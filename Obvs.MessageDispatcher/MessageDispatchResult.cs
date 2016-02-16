using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obvs.MessageDispatcher
{
    public class MessageDispatchResult<TMessage>
    {
        public MessageDispatchResult(TMessage message, bool handled)
        {
            this.Message = message;
            this.Handled = handled;
        }

        public TMessage Message
        {
            get;
            private set;
        }

        public bool Handled
        {
            get;
            private set;
        }
    }
}
