using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ajustee
{
    internal class FakeSubscriber : Subscriber
    {
        public List<KeyValuePair<string, IDictionary<string, string>>> SubscribeInputs = new List<KeyValuePair<string, IDictionary<string, string>>>();

        public FakeSubscriber(AjusteeConnectionSettings settings)
            : base(settings)
        { }

        protected override Task ConnectAsync(string path, IDictionary<string, string> properties, CancellationToken cancellationToken)
        {
            SubscribeInputs.Add(new KeyValuePair<string, IDictionary<string, string>>(path, properties));
            return Task.Delay(1);
        }

        protected override void OnReceiveMessage(ReceiveMessage message)
        {
            throw new NotImplementedException();
        }

        protected override Task ReceiveAsync(MemoryStream stream, CancellationToken cancellationToken)
        {
            return Task.Delay(1);
        }

        protected override Task SendCommandAsync(WsCommand command, CancellationToken cancellationToken)
        {
            switch (command)
            {
                case WsSubscribeCommand _subCommand:
                    SubscribeInputs.Add(new KeyValuePair<string, IDictionary<string, string>>(_subCommand.Path, _subCommand.Properties));
                    break;
            }
            return Task.FromResult(0);
        }
    }
}
