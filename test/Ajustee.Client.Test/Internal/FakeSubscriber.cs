using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using static Ajustee.Helper;

namespace Ajustee
{
    internal class FakeSubscriber : Subscriber
    {
        public List<string> Output = new List<string>();
        private Queue<string> m_ReceiveScenarioSteps = new Queue<string>();
        private SemaphoreSlim m_ReceiveScenarioWaiter;

        public FakeSubscriber(AjusteeConnectionSettings settings, string[] receiveScenarioSteps)
            : base(settings)
        {
            if (receiveScenarioSteps != null)
            {
                foreach (var _step in receiveScenarioSteps) m_ReceiveScenarioSteps.Enqueue(_step);
                m_ReceiveScenarioWaiter = new SemaphoreSlim(0, 1);
            }
        }

        public async Task WaitReceiveScenario()
        {
            if (m_ReceiveScenarioWaiter != null)
                await m_ReceiveScenarioWaiter.WaitAsync();
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

        protected override async Task ReceiveAsync(MemoryStream stream, CancellationToken cancellationToken)
        {
            if (m_ReceiveScenarioSteps == null || m_ReceiveScenarioSteps.Count == 0)
            {
                m_ReceiveScenarioWaiter.Release();
                throw new TaskCanceledException("Receive completed");
            }

            while (m_ReceiveScenarioSteps.Count > 0)
            {
                var _scenario = m_ReceiveScenarioSteps.Dequeue();

                var _match = Regex.Match(_scenario, @"Receive\s+(?<type>config\skeys|info|reset)(?:\s+(?<data>.+?))?\s+after\s+(?<after>\d+)\s+ms", RegexOptions.IgnoreCase);

                string _action = null;
                object _data = null;
                switch (_match.Groups["type"].Value.ToLowerInvariant())
                {
                    case "config keys":
                        _action = ReceiveMessage.ConfigKeys;
                        _data = JsonSerializer.Deserialize<IEnumerable<ConfigKey>>(_match.Groups["data"].Value);
                        break;

                    case "info":
                        _action = ReceiveMessage.Info;
                        _data = _match.Groups["data"].Value;
                        break;

                    case "reset":
                        _action = ReceiveMessage.Reset;
                        _data = null;
                        break;
                }

                await Task.Delay(int.Parse(_match.Groups["after"].Value), cancellationToken);

                var _receiveMessage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ReceiveMessage { Action = _action, Data = _data }));
                stream.Write(_receiveMessage, 0, _receiveMessage.Length);

                await Task.Yield();
            }
        }

        protected override void OnReceiveMessage(ReceiveMessage message)
        {
            try
            {
                object _data = null;
                switch (message.Action)
                {
                    case ReceiveMessage.ConfigKeys:
                        _data = JsonSerializer.Serialize(message.Data);
                        break;

                    default:
                        _data = message.Data;
                        break;
                }
                Output.Add($"{nameof(OnReceiveMessage)}({message.Action}, {_data}): OK");
            }
            catch
            {
                Output.Add($"{nameof(OnReceiveMessage)}({message.Action}): FAILED");
            }
        }
    }
}
