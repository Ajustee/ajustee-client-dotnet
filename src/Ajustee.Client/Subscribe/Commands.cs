using System;
using System.Collections.Generic;
using System.Text;

using static Ajustee.Helper;

namespace Ajustee
{
    internal abstract class WsCommand
    {
        public string action { get; }
        public object data { get; private set; }

        public WsCommand(string action)
        {
            this.action = action;
        }

        public ArraySegment<byte> GetBinary()
        {
            return new ArraySegment<byte>(MessageEncoding.GetBytes(JsonSerializer.Serialize(this)));
        }

        protected void SetData(object data)
        {
            this.data = data;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }

    internal sealed class WsSubscribeCommand : WsCommand
    {
        public WsSubscribeCommand(string path, IDictionary<string, string> properties)
            : base("subscribe")
        {
            SetData(new { path, props = properties });
        }
    }

    internal sealed class WsUnsubscribeCommand : WsCommand
    {
        public WsUnsubscribeCommand(string path)
            : base("unsubscribe")
        {
            SetData(new { path });
        }
    }
}
