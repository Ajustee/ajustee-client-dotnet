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
        private readonly FakeAjusteeClient m_Client;
        public Queue<string> SubscribeScenarioSteps = new Queue<string>();
        public Queue<string> ReceiveScenarioSteps = new Queue<string>();
        private Task SubscribeTask;
        private SemaphoreSlim ReceiveScenarioWaiter = new SemaphoreSlim(0, 1);
        private bool? m_CurrentSubscribeFailed;


        public FakeSubscriber(AjusteeConnectionSettings settings, FakeAjusteeClient client)
            : base(settings)
        {
            m_Client = client;
        }

        private async Task SubscribeScenarioImpl()
        {
            while (SubscribeScenarioSteps.Count > 0)
            {
                var _scenario = SubscribeScenarioSteps.Dequeue();

                var _match = Regex.Match(_scenario, @"Subscribe\s+(?<result>failed|success)\s+on\s+(?<path>.+?)(?:\s+with\s+(?<props>.+?))?\s+after\s+(?<after>\d+)\s+ms", RegexOptions.IgnoreCase);
                var _failed = _match.Groups["result"].Value == "failed";
                if (!int.TryParse(_match.Groups["after"].Value, out var _delay)) _delay = 1;
                var _path = _match.Groups["path"].Value;
                var _propsGroup = _match.Groups["props"];
                var _props = _propsGroup.Success ? JsonSerializer.Deserialize<IDictionary<string, string>>(_propsGroup.Value) : null;

                await Task.Delay(_delay);
                m_CurrentSubscribeFailed = _failed;
                try
                {
                    await SubscribeAsync(_path, _props);
                }
                finally
                {
                    m_CurrentSubscribeFailed = null;
                }
            }
        }

        public async Task WaitScenario()
        {
            var _waiters = new List<Task>(2);
            if (SubscribeScenarioSteps.Count != 0)
            {
                SubscribeTask = Task.Run(SubscribeScenarioImpl);
                _waiters.Add(SubscribeTask);
            }
            if (ReceiveScenarioWaiter != null) _waiters.Add(ReceiveScenarioWaiter.WaitAsync());
            await Task.WhenAll(_waiters);
        }

        protected override async Task ConnectAsync(string path, IDictionary<string, string> properties, CancellationToken cancellationToken)
        {
            var _result = $"{nameof(ConnectAsync)}({path}, {(properties == null ? "" : JsonSerializer.Serialize(properties))})";
            try
            {
                await Task.Delay(1);
                if (m_CurrentSubscribeFailed == true)
                    throw new Exception("Connection failed");

                _result += ": OK";
            }
            catch (Exception)
            {
                _result += ": FAILED";
                throw;
            }
            finally
            {
                m_Client.Output.Add(_result);
            }
        }

        protected override async Task SendCommandAsync(WsCommand command, CancellationToken cancellationToken)
        {
            var _result = $"{nameof(SendCommandAsync)}";
            try
            {
                switch (command)
                {
                    case WsSubscribeCommand _subCommand:
                        {
                            _result += $"_Subscribe({_subCommand.Path}, {(_subCommand.Properties == null ? "" : JsonSerializer.Serialize(_subCommand.Properties))})";
                            await Task.Delay(1);

                            if (m_CurrentSubscribeFailed == true)
                                throw new Exception("Send subscribe failed");

                            _result += ": OK";
                            break;
                        }
                }
            }
            catch (Exception)
            {
                _result += ": FAILED";
                throw;
            }
            finally
            {
                m_Client.Output.Add(_result);
            }
        }

        protected override async Task ReceiveAsync(MemoryStream stream, CancellationToken cancellationToken)
        {
            if (ReceiveScenarioSteps == null || ReceiveScenarioSteps.Count == 0)
            {
                ReceiveScenarioWaiter.Release();
                throw new TaskCanceledException("Receive completed");
            }

            while (ReceiveScenarioSteps.Count > 0)
            {
                var _scenario = ReceiveScenarioSteps.Dequeue();

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
                m_Client.Output.Add($"{nameof(OnReceiveMessage)}({message.Action}, {_data}): OK");
            }
            catch
            {
                m_Client.Output.Add($"{nameof(OnReceiveMessage)}({message.Action}): FAILED");
            }
        }
    }
}
