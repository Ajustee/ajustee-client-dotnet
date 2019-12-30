using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using static Ajustee.Helper;

namespace Ajustee
{
    internal class FakeSubscriber : Subscriber
    {
        public List<string> Output = new List<string>();
        private Queue<string> m_ReceiveScenarioSteps = new Queue<string>();

        public FakeSubscriber(AjusteeConnectionSettings settings)
            : base(settings)
        { }

        public void SetReceiveScenario(params string[] steps)
        {
            foreach (var _step in steps) m_ReceiveScenarioSteps.Enqueue(_step);
        }

        protected override async Task ConnectAsync(string path, IDictionary<string, string> properties, CancellationToken cancellationToken)
        {
            var _result = $"{nameof(ConnectAsync)}({path}, {(properties == null ? "" : JsonSerializer.Serialize(properties))})";
            try
            {
                await Task.Delay(1);
                _result += ": OK";
            }
            catch (Exception)
            {
                _result += ": FAILED";
            }
            finally
            {
                Output.Add(_result);
            }
        }

        protected override Task SendCommandAsync(WsCommand command, CancellationToken cancellationToken)
        {
            switch (command)
            {
                case WsSubscribeCommand _subCommand:
                    Output.Add($"{nameof(SendCommandAsync)}({_subCommand.Path}, {(_subCommand.Properties == null ? "" : JsonSerializer.Serialize(_subCommand.Properties))}): OK");
                    break;
            }
            return Task.FromResult(0);
        }

        protected override Task ReceiveAsync(MemoryStream stream, CancellationToken cancellationToken)
        {
            return Task.Delay(1);
        }

        protected override void OnReceiveMessage(ReceiveMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
